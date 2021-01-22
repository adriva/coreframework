using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Adriva.Extensions.Worker
{
    internal class ScheduledJobsHost : BackgroundService
    {
        internal class ScheduledItem
        {
            public string Expression { get; }

            public MethodInfo Method { get; }

            public IExpressionParser Parser { get; }

            public ScheduledItem(string expression, MethodInfo method, IExpressionParser parser)
            {
                this.Expression = expression;
                this.Method = method;
                this.Parser = parser;
            }
        }

        private readonly Timer Timer;
        private readonly CancellationTokenSource CancellationTokenSource;
        private readonly IServiceProvider ServiceProvider;
        private readonly IList<ScheduledItem> ScheduledItems = new List<ScheduledItem>();
        private readonly ConcurrentDictionary<IntPtr, object> MethodOwners = new ConcurrentDictionary<IntPtr, object>();
        private readonly ILogger Logger;

        private DateTime LastRunDate;
        private long RunningItemCount = 0;

        public ScheduledJobsHost(IServiceProvider serviceProvider, ILogger<ScheduledJobsHost> logger)
        {
            this.ServiceProvider = serviceProvider;
            this.Logger = logger;
            this.CancellationTokenSource = new CancellationTokenSource();
            this.Timer = new Timer(this.OnTimerElapsed, null, Timeout.Infinite, 5000);
        }

        private IEnumerable<MethodInfo> ResolveScheduledMethods()
        {
            return from type in ReflectionHelpers.FindTypes(t => t.IsClass && !t.IsAbstract && !t.IsSpecialName)
                   from method in type.GetMethods()
                   where !method.IsSpecialName && method.GetCustomAttributes<ScheduleAttribute>(true).Any()
                   select method;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
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

                    this.ScheduledItems.Add(new ScheduledItem(scheduleAttribute.Expression, method, parserCache[scheduleAttribute.ExpressionParserType]));
                }
            }

            parserCache.Clear();

            this.LastRunDate = DateTime.UtcNow;
            foreach (var scheduledItem in this.ScheduledItems)
            {
                DateTime? nextRunDate = scheduledItem.Parser.GetNext(this.LastRunDate, scheduledItem.Expression);

                if (!nextRunDate.HasValue) continue;

                this.Logger.LogInformation($"Next scheduled run for '{scheduledItem.Method.Name}' is at '{nextRunDate.Value}' Local Time.");
            }
            while (0 != DateTime.Now.Second % 5)
            {
                await Task.Delay(250);
            }
            this.Timer.Change(0, 5000);
        }

        private void OnTimerElapsed(object state)
        {
            foreach (var scheduledItem in this.ScheduledItems)
            {
                DateTime? nextRunDate = scheduledItem.Parser.GetNext(this.LastRunDate, scheduledItem.Expression);
                if (!nextRunDate.HasValue) continue;

                if (nextRunDate.Value <= DateTime.Now)
                {
                    ThreadPool.QueueUserWorkItem(this.SafeRunItem, scheduledItem);
                }
            }

            this.LastRunDate = DateTime.UtcNow;
        }

        private async void SafeRunItem(object state)
        {
            ScheduledItem scheduledItem = null;
            try
            {
                scheduledItem = state as ScheduledItem;
                if (null == scheduledItem) return;
                Interlocked.Increment(ref this.RunningItemCount);
                await this.RunItem(scheduledItem);
            }
            catch (Exception fatalError)
            {
                this.Logger.LogError(fatalError, $"Failed to execute scheduled job '{scheduledItem.Method.Name}'.");
            }
            finally
            {
                Interlocked.Decrement(ref this.RunningItemCount);
                DateTime? nextRunDate = scheduledItem.Parser.GetNext(this.LastRunDate, scheduledItem.Expression);
                if (nextRunDate.HasValue)
                {
                    this.Logger.LogInformation($"Next scheduled run for '{scheduledItem.Method.Name}' is at '{nextRunDate.Value}' UTC.");
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            this.Timer.Change(Timeout.Infinite, Timeout.Infinite);
            this.CancellationTokenSource.Cancel();

            await base.StopAsync(cancellationToken);

            SpinWait.SpinUntil(() =>
            {
                return 0 == Interlocked.Read(ref this.RunningItemCount);
            }, 30000);
        }

        private async Task RunItem(ScheduledItem scheduledItem)
        {
            object ownerType = null;

            if (!scheduledItem.Method.IsStatic)
            {
                ownerType = this.MethodOwners.GetOrAdd<object>(scheduledItem.Method.MethodHandle.Value, (key, state) =>
                {
                    var si = state as ScheduledItem;
                    return ActivatorUtilities.CreateInstance(this.ServiceProvider, si.Method.DeclaringType);
                }, scheduledItem);
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

            object returnValue = scheduledItem.Method.Invoke(ownerType, parameters);

            if (returnValue is Task returnTask)
            {
                await returnTask;
            }

            this.Logger.LogInformation($"Executed scheduled job '{scheduledItem.Method.Name}'.");
        }

        public override void Dispose()
        {
            foreach (var ownerType in this.MethodOwners.Values)
            {
                if (ownerType is IDisposable disposable) disposable.Dispose();
                else if (ownerType is IAsyncDisposable asyncDisposable)
                {
                    asyncDisposable.DisposeAsync().AsTask().Wait();
                }
            }

            base.Dispose();
            this.Timer.Change(Timeout.Infinite, Timeout.Infinite);
            this.Timer.Dispose();
        }
    }
}
