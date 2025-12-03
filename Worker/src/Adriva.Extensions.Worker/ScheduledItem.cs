using System;
using System.Reflection;
using Newtonsoft.Json;

namespace Adriva.Extensions.Worker
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed class ScheduledItem
    {
        public string Expression { get; }

        public MethodInfo Method { get; }

        public IExpressionParser Parser { get; }

        [JsonProperty]
        public bool IsSingleton { get; private set; }

        [JsonProperty]
        public bool ShouldRunOnStartup { get; set; }

        [JsonProperty]
        public string JobId { get; private set; }

        public DateTime? NextScheduledDate { get; set; }

        public bool IsReadyToRun { get; set; }

        [JsonConstructor]
        private ScheduledItem()
        {

        }

        public ScheduledItem(string jobId, string expression, MethodInfo method, IExpressionParser parser, bool isSingleton)
        {
            this.JobId = jobId;
            this.Expression = expression;
            this.Method = method;
            this.Parser = parser;
            this.IsSingleton = isSingleton;
        }
    }
}
