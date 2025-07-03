using System;
using Microsoft.Extensions.Primitives;

namespace Adriva.Extensions.Caching.Memory
{
    internal sealed class DependencyChangeToken : IChangeToken
    {
        public bool HasChanged { get; private set; }

        public bool ActiveChangeCallbacks => false;

        public IDisposable RegisterChangeCallback(Action<object> callback, object state)
        {
            throw new NotImplementedException();
        }

        internal void NotifyChanged()
        {
            this.HasChanged = true;
        }
    }
}