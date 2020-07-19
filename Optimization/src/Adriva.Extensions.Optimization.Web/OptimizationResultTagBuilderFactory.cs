using System;
using System.Collections.Generic;
using Adriva.Extensions.Optimization.Abstractions;

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

        public IOptimizationResultTagBuilder GetBuilder(AssetFileExtension extension)
        {
            if (!this.BuilderCache.ContainsKey(extension)) throw new ArgumentOutOfRangeException($"Could not locate a tag builder for extension '{extension}'.");
            return this.BuilderCache[extension];
        }
    }
}
