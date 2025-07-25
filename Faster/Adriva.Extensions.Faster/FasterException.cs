using System;
using System.Runtime.Serialization;
namespace Adriva.Extensions.Faster
{
    [Serializable]
    public class FasterException : Exception
    {
        public FasterException() { }

        public FasterException(string message) : base(message) { }

        public FasterException(string message, System.Exception inner) : base(message, inner) { }
    }

    [Serializable]
    public class FasterConflictException : FasterException
    {
        public FasterConflictException()
            : base()
        {
        }

        public FasterConflictException(string message)
            : base(message)
        {
        }

        public FasterConflictException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}