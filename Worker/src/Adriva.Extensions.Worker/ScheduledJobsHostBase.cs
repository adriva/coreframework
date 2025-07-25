using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Adriva.Extensions.Worker
{
    public abstract class ScheduledJobsHostBase : BackgroundService, IScheduledJobsHost
    {
        protected IList<ScheduledItem> ScheduledItems { get; } = new List<ScheduledItem>();

        protected IServiceProvider ServiceProvider { get; private set; }

        protected ILogger Logger { get; private set; }

        protected CancellationTokenSource CancellationTokenSource { get; private set; }

        protected IScheduledJobEvents Events { get; private set; }

        protected IWorkerLock WorkerLock { get; private set; }

        protected bool IsDisposed { get; private set; }

        protected ScheduledJobsHostBase(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;

            var loggerFactory = this.ServiceProvider.GetRequiredService<ILoggerFactory>();
            this.Logger = loggerFactory.CreateLogger(this.GetType().FullName);

            this.CancellationTokenSource = new CancellationTokenSource();
            this.Events = this.ServiceProvider.GetService<IScheduledJobEvents>();
            this.WorkerLock = this.ServiceProvider.GetService<IWorkerLock>() ?? new NullLock();
        }

        public static string GenerateJobId(MethodInfo methodInfo)
        {
            StringBuilder buffer = new StringBuilder();

            buffer.AppendFormat("{0}:", ReflectionHelpers.GetNormalizedName(methodInfo.DeclaringType));
            buffer.AppendFormat("{0}", ReflectionHelpers.GetNormalizedName(methodInfo));

            return buffer.ToString();
        }

        public virtual IEnumerable<MethodInfo> ResolveScheduledMethods()
        {
            return from type in ReflectionHelpers.FindTypes(t => t.IsClass && !t.IsAbstract && !t.IsSpecialName)
                   from method in type.GetMethods()
                   where !method.IsSpecialName && method.GetCustomAttributes<ScheduleAttribute>(true).Any()
                   select method;
        }

        public bool TryResolveMethod(ScheduledItem scheduledItem, out MethodInfo methodInfo)
        {
            methodInfo = null;

            if (string.IsNullOrWhiteSpace(scheduledItem?.JobId))
            {
                return false;
            }

            methodInfo = this.ScheduledItems.FirstOrDefault(si => 0 == string.Compare(scheduledItem.JobId, si.JobId))?.Method;
            return null != methodInfo;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var methods = this.ResolveScheduledMethods();

            IDictionary<Type, IExpressionParser> parserCache = new Dictionary<Type, IExpressionParser>();

            foreach (var method in methods)
            {
                var scheduleAttributes = method.GetCustomAttributes<ScheduleAttribute>();
                foreach (var scheduleAttribute in scheduleAttributes)
                {
                    if (!parserCache.ContainsKey(scheduleAttribute.ExpressionParserType))
                    {
                        parserCache[scheduleAttribute.ExpressionParserType] = (IExpressionParser)ActivatorUtilities.CreateInstance(this.ServiceProvider, scheduleAttribute.ExpressionParserType);
                    }
                    string jobId = ScheduledJobsHost.GenerateJobId(method);
                    var scheduledItem = new ScheduledItem(jobId, scheduleAttribute.Expression, method, parserCache[scheduleAttribute.ExpressionParserType], scheduleAttribute.IsSingleton);
                    scheduledItem.ShouldRunOnStartup = scheduleAttribute.RunOnStartup;
                    this.ScheduledItems.Add(scheduledItem);
                }
            }

            parserCache.Clear();

            return Task.CompletedTask;
        }

        protected virtual async Task<LockStatus> RunItemAsync(ScheduledItemInstance scheduledItemInstance)
        {
            object ownerInstance = null;
            ScheduledItem scheduledItem = scheduledItemInstance.ScheduledItem;

            if (!scheduledItem.Method.IsStatic)
            {
                ownerInstance = ActivatorUtilities.CreateInstance(this.ServiceProvider, scheduledItem.Method.DeclaringType);
            }

            var parameterInfoItems = scheduledItem.Method.GetParameters();
            object[] parameters = new object[parameterInfoItems.Length];

            for (int loop = 0; loop < parameters.Length; loop++)
            {
                if (parameterInfoItems[loop].ParameterType == typeof(CancellationToken))
                {
                    parameters[loop] = this.CancellationTokenSource.Token;
                }
                else
                {
                    parameters[loop] = ActivatorUtilities.CreateInstance(this.ServiceProvider, parameterInfoItems[loop].ParameterType);
                }
            }

            this.Logger.LogInformation($"Executing scheduled job '{scheduledItem.Method.Name}'.");

            LockStatus lockStatus = new LockStatus(scheduledItemInstance.InstanceId, true);
            try
            {

                if (scheduledItemInstance.ScheduledItem.IsSingleton)
                {
                    lockStatus = await this.WorkerLock.AcquireLockAsync(scheduledItemInstance.ScheduledItem.JobId, scheduledItemInstance.InstanceId, TimeSpan.Zero);

                    if (lockStatus.HasLock)
                    {
                        this.Logger.LogInformation($"Job instance '{scheduledItemInstance.InstanceId}' ({ReflectionHelpers.GetNormalizedName(scheduledItemInstance.ScheduledItem.Method)}) has acquired lock.");
                    }
                    else
                    {
                        this.Logger.LogWarning($"Job instance '{scheduledItemInstance.InstanceId}' ({ReflectionHelpers.GetNormalizedName(scheduledItemInstance.ScheduledItem.Method)}) has failed to acquire a lock.");
                        return lockStatus;
                    }
                }

                if (null != this.Events)
                {
                    await this.Events.ExecutingAsync(ownerInstance, scheduledItemInstance.InstanceId, scheduledItem.Method);
                }

                object returnValue = scheduledItem.Method.Invoke(ownerInstance, parameters);

                if (returnValue is Task returnTask)
                {
                    await returnTask;
                }

                if (null != this.Events)
                {
                    await this.Events.ExecutedAsync(ownerInstance, scheduledItemInstance.InstanceId, scheduledItem.Method, null);
                }
            }
            catch (Exception error)
            {
                if (null != this.Events)
                {
                    await this.Events.ExecutedAsync(ownerInstance, scheduledItemInstance.InstanceId, scheduledItem.Method, error);
                }

                throw;
            }
            finally
            {
                if (ownerInstance is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                else if (ownerInstance is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync();
                }

                await this.WorkerLock.ReleaseLockAsync(scheduledItem.JobId, scheduledItemInstance.InstanceId);
            }
            this.Logger.LogInformation($"Executed scheduled job '{scheduledItem.Method.Name}'.");
            return lockStatus;
        }

        public virtual async Task<LockStatus> RunAsync(MethodInfo methodInfo)
        {
            if (null == methodInfo)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            var scheduledItem = this.ScheduledItems.FirstOrDefault(s => s.Method == methodInfo);

            if (null == scheduledItem)
            {
                throw new InvalidOperationException($"Method '{ReflectionHelpers.GetNormalizedName(methodInfo)}' is not a valid scheduled job.");
            }

            string instanceId = Guid.NewGuid().ToString();
            ScheduledItemInstance scheduledItemInstance = new ScheduledItemInstance(scheduledItem, instanceId);

            return await this.RunItemAsync(scheduledItemInstance);
        }

        public override void Dispose()
        {
            if (!this.IsDisposed)
            {
                this.Logger.LogDebug("Disposing 'ScheduledJobsHost' instance.");
                base.Dispose();
                this.CancellationTokenSource.Dispose();
                this.CancellationTokenSource = null;
                this.IsDisposed = true;
            }
        }
    }
}
