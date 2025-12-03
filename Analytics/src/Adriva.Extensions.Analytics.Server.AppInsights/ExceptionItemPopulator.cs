using System;
using System.Linq;
using Adriva.Extensions.Analytics.Server.AppInsights.Contracts;
using Adriva.Extensions.Analytics.Server.Entities;

namespace Adriva.Extensions.Analytics.Server.AppInsights
{
    internal sealed class ExceptionItemPopulator : AnalyticsItemPopulator
    {
        public override string TargetKey => "AppExceptions";

        public override bool TryPopulate(Envelope envelope, ref AnalyticsItem analyticsItem)
        {
            if (!(envelope.EventData is ExceptionData exceptionData)) return false;
            if (null == exceptionData.Exceptions || 0 == exceptionData.Exceptions.Count) return false;

            foreach (var exceptionDetails in exceptionData.Exceptions)
            {
                ExceptionItem exceptionItem = new ExceptionItem();
                if (exceptionData.Properties.TryGetValue("RequestId", out string requestId)) exceptionItem.RequestId = requestId;
                if (exceptionData.Properties.TryGetValue("CategoryName", out string categoryName)) exceptionItem.Category = categoryName;
                if (exceptionData.Properties.TryGetValue("RequestPath", out string path)) exceptionItem.Path = path;
                if (exceptionData.Properties.TryGetValue("ConnectionId", out string connectionId)) exceptionItem.ConnectionId = connectionId;
                if (exceptionData.Properties.TryGetValue("FormattedMessage", out string message)) exceptionItem.Message = message;
                if (exceptionData.Properties.TryGetValue("EventName", out string eventName)) exceptionItem.Name = eventName;

                exceptionItem.ExceptionId = exceptionDetails.Id;
                exceptionItem.ExceptionMessage = exceptionDetails.Message;
                exceptionItem.ExceptionType = exceptionDetails.TypeName;

                if (null != exceptionDetails.ParsedStack && 0 < exceptionDetails.ParsedStack.Count)
                {
                    exceptionItem.StackTrace = string.Join(Environment.NewLine, exceptionDetails.ParsedStack.Select(s => s.ToString()));
                }

                analyticsItem.Exceptions.Add(exceptionItem);
            }

            return true;
        }
    }
}