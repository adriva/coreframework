using System;

namespace Adriva.Extensions.TextSearch
{
    public class TextSearchManagerUpdatePolicy
    {

        public static readonly TextSearchManagerUpdatePolicy Default = new TextSearchManagerUpdatePolicy(100);

        public int QueueSizeLimit { get; private set; }

        public TextSearchManagerUpdatePolicy(int queueSizeLimit)
        {
            this.QueueSizeLimit = Math.Max(1, queueSizeLimit);
        }

    }
}
