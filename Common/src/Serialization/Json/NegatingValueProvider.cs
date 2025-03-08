using Newtonsoft.Json.Serialization;

namespace Adriva.Common.Core.Serialization.Json
{
    public sealed class NegatingValueProvider : IValueProvider
    {
        private readonly IValueProvider BaseProvider;

        public NegatingValueProvider(IValueProvider baseProvider)
        {
            this.BaseProvider = baseProvider;
        }

        public object GetValue(object target)
        {
            object value = this.BaseProvider.GetValue(target);
            if (value is bool boolValue) return !boolValue;
            return value;
        }

        public void SetValue(object target, object value)
        {
            if (value is bool boolValue)
            {
                this.BaseProvider.SetValue(target, !boolValue);
                return;
            }

            this.BaseProvider.SetValue(target, value);
        }
    }
}