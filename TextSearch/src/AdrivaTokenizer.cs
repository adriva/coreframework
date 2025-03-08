using Lucene.Net.Analysis.Core;
using Lucene.Net.Util;
using System.IO;

namespace Adriva.Extensions.TextSearch
{
    internal sealed class AdrivaTokenizer : LetterTokenizer
    {
        public AdrivaTokenizer(LuceneVersion matchVersion, TextReader reader)
            : base(matchVersion, reader)
        {
        }

        protected override bool IsTokenChar(int c)
        {
            char character = (char)c;
            return base.IsTokenChar(c) || char.IsDigit((char)c) || '(' == character || ')' == character;
        }
    }
}
