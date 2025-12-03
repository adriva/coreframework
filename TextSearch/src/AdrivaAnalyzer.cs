using Lucene.Net.Analysis;
using Lucene.Net.Util;
using System.IO;

namespace Adriva.Extensions.TextSearch
{
    public class AdrivaAnalyzer : Lucene.Net.Analysis.Analyzer
    {
        private readonly LuceneVersion LuceneVersion;

        public AdrivaAnalyzer(LuceneVersion luceneVersion) : base()
        {
            this.LuceneVersion = luceneVersion;
        }

        protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
        {
            var tokenizer = new AdrivaTokenizer(this.LuceneVersion, reader);
            var components = new TokenStreamComponents(tokenizer);
            return components;
        }
    }
}
