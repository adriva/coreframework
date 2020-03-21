using System;
using System.Runtime.Serialization;

namespace Adriva.Extensions.Optimization.Abstractions
{
    [Serializable]
    public class OptimizationException : Exception
    {
        public OptimizationException() { }

        public OptimizationException(string message) : base(message) { }

        public OptimizationException(string message, Exception inner) : base(message, inner) { }

        protected OptimizationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}