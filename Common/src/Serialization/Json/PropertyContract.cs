namespace Adriva.Common.Core.Serialization.Json
{
    public sealed class PropertyContract
    {
        public string OverridenName { get; private set; }

        public bool ShouldNegate { get; set; }

        public bool IgnoreDefaultValue { get; set; }

        public PropertyContract(string overridenName)
        {
            this.OverridenName = overridenName;
        }
    }
}