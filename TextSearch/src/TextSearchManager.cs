using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Adriva.Extensions.TextSearch
{
    public abstract class TextSearchManager : IDisposable
    {
        private long IsSearcherReady = 0;
        private long ActiveSearchCount = 0;
        private Lucene.Net.Store.Directory SearchDirectory;
        private DirectoryReader Reader;
        private IndexSearcher Searcher;

        public bool IsSearcherCreated
        {
            get
            {
                return 1 == Interlocked.Read(ref this.IsSearcherReady);
            }
        }

        public string IndexPath { get; private set; }

        protected TextSearchManager(string indexPath)
        {
            this.IndexPath = indexPath;
        }

        protected virtual bool CanReuseIndex(DirectoryInfo directory)
        {
            return false;
        }

        protected abstract Task<RawDataResult> GetRawDataAsync(object lastRowId);

        protected abstract Document ResolveDocument(object dataItem);

        protected virtual void OnIndexCreated() { }

        protected virtual void OnIndexCreating() { }

        public Task CreateIndexFileAsync(bool forceCreate)
        {
            return this.CreateIndexFileAsync(forceCreate, false);
        }

        private async Task CreateIndexFileAsync(bool forceCreate, bool isRecycling)
        {
            SpinWait.SpinUntil(() => 0 == Interlocked.Read(ref this.ActiveSearchCount));

            if (!isRecycling || !(this.SearchDirectory is RAMDirectory))
            {
                Interlocked.Exchange(ref this.IsSearcherReady, 0);
            }

            this.OnIndexCreating();

            DirectoryInfo indexDirectoryInfo = new DirectoryInfo(this.IndexPath);

            if (!forceCreate && this.CanReuseIndex(indexDirectoryInfo)) return;

            var files = indexDirectoryInfo.GetFiles();

            Array.ForEach(files, file =>
            {
                if (!file.Name.Equals("placeholder.txt", StringComparison.OrdinalIgnoreCase)
                    && !file.Name.Equals("work.lock", StringComparison.OrdinalIgnoreCase))
                {
                    file.Delete();
                }
            });

            using (SimpleFSDirectory fsDirectory = new SimpleFSDirectory(indexDirectoryInfo))
            {
                using (var analyzer = new AdrivaAnalyzer(LuceneVersion.LUCENE_48))
                {
                    IndexWriterConfig indexWriterConfig = new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer) { };
                    using (IndexWriter writer = new IndexWriter(fsDirectory, indexWriterConfig))
                    {
                        RawDataResult result = new RawDataResult
                        {
                            HasMore = false
                        };

                        do
                        {
                            result = await this.GetRawDataAsync(result.LastRowId);

                            var rawDataEnumerator = result.Items.GetEnumerator();

                            while (rawDataEnumerator.MoveNext())
                            {
                                Document document = this.ResolveDocument(rawDataEnumerator.Current);
                                if (null != document) writer.AddDocument(document, analyzer);
                            }

                        } while (result.HasMore);

                        writer.Commit();
                    }
                }
            }

            this.OnIndexCreated();
        }

        public void CreateSearcher(bool storeIndexInMemory = false)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(this.IndexPath);

            if (null != this.Searcher)
            {
                this.Searcher = null;
            }

            if (null != this.Reader)
            {
                this.Reader.Dispose();
                this.Reader = null;
            }

            if (null != this.SearchDirectory)
            {
                this.SearchDirectory.Dispose();
                this.SearchDirectory = null;
            }

            if (storeIndexInMemory)
            {
                this.SearchDirectory = new RAMDirectory(new SimpleFSDirectory(directoryInfo), IOContext.DEFAULT);
            }
            else
            {
                this.SearchDirectory = new SimpleFSDirectory(directoryInfo);
            }

            this.Reader = DirectoryReader.Open(this.SearchDirectory);
            this.Searcher = new IndexSearcher(this.Reader);
            Interlocked.Exchange(ref this.IsSearcherReady, 1);
        }

        public IEnumerable<T> Search<T>(Query query, int count, Sort sort, Func<Document, ScoreDoc, T> func)
        {
            if (1 != Interlocked.Read(ref this.IsSearcherReady)) return Array.Empty<T>();

            Interlocked.Increment(ref this.ActiveSearchCount);

            try
            {
                TopFieldCollector collector = TopFieldCollector.Create(sort, count, true, true, false, false);
                this.Searcher.Search(query, collector);
                return collector.GetTopDocs().ScoreDocs.Select<ScoreDoc, T>(
                    scoreDoc =>
                    {
                        var document = this.Searcher.Doc(scoreDoc.Doc);
                        return func(document, scoreDoc);
                    }
                );
            }
            finally
            {
                Interlocked.Decrement(ref this.ActiveSearchCount);
            }
        }

        public virtual async Task RecycleAsync(bool forceCreateIndex = false, bool storeIndexInMemory = false)
        {
            await this.CreateIndexFileAsync(forceCreateIndex, true);
            this.CreateSearcher(storeIndexInMemory);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Interlocked.Exchange(ref this.IsSearcherReady, 0);
                this.Reader?.Dispose();
                this.SearchDirectory?.Dispose();
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

}
