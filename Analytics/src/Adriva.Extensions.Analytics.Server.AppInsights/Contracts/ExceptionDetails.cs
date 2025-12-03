using System.Collections.Generic;
using Newtonsoft.Json;

namespace Adriva.Extensions.Analytics.Server.AppInsights.Contracts
{
    public partial class ExceptionDetails
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("outerId")]
        public int OuterId { get; set; }

        [JsonProperty("typeName")]
        public string TypeName { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("hasFullStack")]
        public bool HasFullStack { get; set; }

        [JsonProperty("stack")]
        public string Stack { get; set; }

        [JsonProperty("parsedStack")]
        public List<StackFrame> ParsedStack { get; set; }

        public ExceptionDetails()
            : this("AI.ExceptionDetails", "ExceptionDetails")
        { }

        protected ExceptionDetails(string fullName, string name)
        {
            this.TypeName = string.Empty;
            this.Message = string.Empty;
            this.HasFullStack = true;
            this.Stack = string.Empty;
            this.ParsedStack = new List<StackFrame>();
        }
    }
}