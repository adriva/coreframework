using Newtonsoft.Json;

namespace Adriva.Extensions.Worker
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed class ScheduledItemInstance
    {
        [JsonProperty]
        public ScheduledItem ScheduledItem { get; private set; }

        [JsonProperty]
        public string InstanceId { get; private set; }

        [JsonConstructor]
        private ScheduledItemInstance()
        {

        }

        public ScheduledItemInstance(ScheduledItem scheduledItem, string instanceId)
        {
            this.ScheduledItem = scheduledItem;
            this.InstanceId = instanceId;
        }
    }
}
