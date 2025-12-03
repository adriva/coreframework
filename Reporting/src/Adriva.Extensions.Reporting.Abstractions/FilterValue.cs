namespace Adriva.Extensions.Reporting.Abstractions
{
    public sealed class FilterValue
    {
        public object RawValue { get; private set; }

        public object Value { get; private set; }

        public FilterValue(object rawValue, object value)
        {
            this.RawValue = rawValue;
            this.Value = value;
        }
    }
}