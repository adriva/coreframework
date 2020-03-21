using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Adriva.Extensions.Optimization.Abstractions
{
    public class FileAssetOrderer : IAssetOrderer
    {
        private int MissingOrder;

        protected string FilePath { get; private set; }
        protected List<string> OrderedAssetNames { get; private set; } = new List<string>();

        public FileAssetOrderer(string filePath)
        {
            this.FilePath = filePath;
            this.Initialize();
        }

        private void Initialize()
        {
            var assetNames = (this.LoadAssetNamesInOrder() ?? Array.Empty<string>()).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.ToUpperInvariant());
            this.OrderedAssetNames.AddRange(assetNames);
            this.MissingOrder = this.OrderedAssetNames.Count;
        }

        protected virtual IEnumerable<string> LoadAssetNamesInOrder()
        {
            return File.ReadAllLines(this.FilePath);
        }

        public virtual IEnumerable<Asset> Order(IEnumerable<Asset> assets)
        {
            return assets.OrderBy(x =>
            {
                int index = this.OrderedAssetNames.IndexOf(x.Name.ToUpperInvariant());
                return -1 < index ? index : this.MissingOrder;
            });
        }
    }
}