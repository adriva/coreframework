using Newtonsoft.Json;

namespace Adriva.Extensions.Analytics.Server.AppInsights.Contracts
{
    public partial class PageViewPerfData
        : PageViewData
    {
        [JsonProperty("perfTotal")]
        public string PerfTotal { get; set; }

        [JsonProperty("networkConnect")]
        public string NetworkConnect { get; set; }

        [JsonProperty("sentRequest")]
        public string SentRequest { get; set; }

        [JsonProperty("receivedResponse")]
        public string ReceivedResponse { get; set; }

        [JsonProperty("domProcessing")]
        public string DomProcessing { get; set; }

        public PageViewPerfData()
            : this("AI.PageViewPerfData", "PageViewPerfData")
        { }

        protected PageViewPerfData(string fullName, string name)
        {
            this.PerfTotal = string.Empty;
            this.NetworkConnect = string.Empty;
            this.SentRequest = string.Empty;
            this.ReceivedResponse = string.Empty;
            this.DomProcessing = string.Empty;
        }
    }
}