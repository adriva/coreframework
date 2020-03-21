using System;
using System.Collections.Generic;

namespace Adriva.Extensions.Optimization.Web
{

    internal sealed class OptimizationResultTagBuilderFactory : IOptimizationResultTagBuilderFactory
    {
        private readonly Dictionary<string, IOptimizationResultTagBuilder> BuilderCache = new Dictionary<string, IOptimizationResultTagBuilder>();

        public OptimizationResultTagBuilderFactory(IEnumerable<IOptimizationResultTagBuilder> tagBuilders)
        {
            if (null == tagBuilders) return;

            foreach (var tagBuilder in tagBuilders)
            {
                this.BuilderCache[tagBuilder.Extension] = tagBuilder;
            }
        }

        public IOptimizationResultTagBuilder GetBuilder(string extension)
        {
            if (!this.BuilderCache.ContainsKey(extension)) throw new ArgumentOutOfRangeException($"Could not locate a tag builder for extension '{extension}'.");
            return this.BuilderCache[extension];
        }
    }
}
