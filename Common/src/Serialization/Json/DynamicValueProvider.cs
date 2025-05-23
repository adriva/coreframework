using System;
using System.Linq.Expressions;
using Newtonsoft.Json.Serialization;

namespace Adriva.Common.Core.Serialization.Json
{
    public sealed class DynamicValueProvider<TItem, TValue> : IValueProvider
    {
        private readonly Func<TItem, TValue> GetAccessor;
        private readonly Action<TItem, TValue> SetAccessor;

        public DynamicValueProvider(Func<TItem, TValue> getAccessor, Action<TItem, TValue> setAccessor)
        {
            this.GetAccessor = getAccessor;
            this.SetAccessor = setAccessor;
        }

        public object GetValue(object target)
        {
            if (null == this.GetAccessor) throw new ArgumentNullException(nameof(this.GetAccessor));
            return this.GetAccessor((TItem)target);
        }

        public void SetValue(object target, object value)
        {
            if (null == this.SetAccessor) throw new ArgumentNullException(nameof(this.SetAccessor));
            this.SetAccessor((TItem)target, (TValue)value);
        }
    }
}