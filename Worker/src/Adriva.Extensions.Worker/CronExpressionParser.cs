using System;

namespace Adriva.Extensions.Worker
{
    public class CronExpressionParser : IExpressionParser
    {
        // returns UTC
        public virtual DateTime? GetNext(DateTime afterDate, string expression)
        {
            var cronExpression = Cronos.CronExpression.Parse(expression, Cronos.CronFormat.IncludeSeconds);
            DateTime? nextDate = cronExpression.GetNextOccurrence(afterDate, TimeZoneInfo.Utc);
            if (!nextDate.HasValue) return null;
            int deltaSeconds = ((int)(5 * Math.Round((decimal)nextDate.Value.Second / 5))) - nextDate.Value.Second;
            return nextDate.Value.AddSeconds(deltaSeconds);
        }
    }
}
