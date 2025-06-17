using Newtonsoft.Json;

namespace Adriva.Extensions.Analytics.Server.AppInsights.Contracts
{
    public partial class StackFrame
    {
        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("assembly")]
        public string Assembly { get; set; }

        [JsonProperty("fileName")]
        public string FileName { get; set; }

        [JsonProperty("line")]
        public int Line { get; set; }

        public StackFrame()
            : this("AI.StackFrame", "StackFrame")
        { }

        protected StackFrame(string fullName, string name)
        {
            this.Method = string.Empty;
            this.Assembly = string.Empty;
            this.FileName = string.Empty;
        }

        public override string ToString()
        {
            return $"{this.Method} in '{this.Assembly}' at line {this.Line} in file '{this.FileName}'";
        }
    }
}