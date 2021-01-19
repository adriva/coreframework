using System;
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
        internal readonly struct ScheduledItem
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
        private readonly IServiceProvider ServiceProvider;
        private readonly IList<ScheduledItem> ScheduledItems = new List<ScheduledItem>();
        private readonly ILogger Logger;

        private DateTime LastRunDate;

        public ScheduledJobsHost(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
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
            while (0 != DateTime.Now.Second % 5)
            {
                await Task.Delay(300);
            }
            this.LastRunDate = DateTime.UtcNow;
            this.Timer.Change(0, 5000);
        }

        private void OnTimerElapsed(object state)
        {
            foreach (var scheduledItem in this.ScheduledItems)
            {
                DateTime? nextRunDate = scheduledItem.Parser.GetNext(this.LastRunDate, scheduledItem.Expression);
                if (!nextRunDate.HasValue) continue;
                if (nextRunDate.Value <= DateTime.UtcNow)
                {
                    ThreadPool.QueueUserWorkItem(this.RunItem, scheduledItem);
                }
            }

            this.LastRunDate = DateTime.UtcNow;
        }

        private async void RunItem(object state)
        {
            if (!(state is ScheduledItem scheduledItem)) return;

            object ownerType = null;

            if (!scheduledItem.Method.IsStatic)
            {
                ownerType = ActivatorUtilities.CreateInstance(this.ServiceProvider, scheduledItem.Method.DeclaringType);
            }

            var parameterInfoItems = scheduledItem.Method.GetParameters();
            object[] parameters = new object[parameterInfoItems.Length];

            for (int loop = 0; loop < parameters.Length; loop++)
            {
                parameters[loop] = ActivatorUtilities.CreateInstance(this.ServiceProvider, parameterInfoItems[loop].ParameterType);
            }

            try
            {
                object returnValue = scheduledItem.Method.Invoke(ownerType, parameters);

                if (returnValue is Task returnTask)
                {
                    await returnTask;
                }
            }
            catch (Exception fatalError)
            {
                this.Logger.LogError(fatalError, $"Failed to exeecute scheduled job '{scheduledItem.Method.Name}'.");
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            this.Timer.Change(Timeout.Infinite, Timeout.Infinite);
            this.Timer.Dispose();
        }
    }
}
