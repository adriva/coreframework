using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Adriva.Extensions.Optimization.Abstractions
{
    public class OptimizationOptions
    {
        private Dictionary<int, Type> OrderedFormatters = new Dictionary<int, Type>();

        internal ReadOnlyCollection<Type> Formatters
        {
            get
            {
                var formatterTypes = this.OrderedFormatters.OrderBy(x => x.Key).Select(x => x.Value);
                return new ReadOnlyCollection<Type>(formatterTypes.ToList());
            }
        }

        internal Dictionary<string, List<Type>> TransformChains = new Dictionary<string, List<Type>>();
        internal Type Orderer = typeof(NullAssetOrderer);
        internal object OrdererOptions;

        public OptimizationOptions()
        {

        }

        internal void AddTransformChain(AssetFileExtension assetFileExtension, params Type[] transforms)
        {
            if (null == assetFileExtension) throw new ArgumentNullException(nameof(assetFileExtension));
            if (null == transforms || 0 == transforms.Length) throw new ArgumentException($"No transforms specified for the predicate.");

            Type typeOfITransform = typeof(ITransform);
            if (!transforms.Any(t => typeOfITransform.IsAssignableFrom(t)))
            {
                throw new InvalidCastException("All transform types must implement the interface ITransform.");
            }
            if (transforms.Any(t => !t.IsClass || t.IsAbstract || !t.IsPublic))
            {
                throw new InvalidCastException("All transform types must be concrete public classes.");
            }

            this.TransformChains[assetFileExtension] = transforms.ToList(); // calling this method for the same key will overwrite the existing chain
        }

        internal void AddFormatter(int order, Type formatterType)
        {
            if (null == formatterType) return;
            if (!formatterType.IsPublic || !formatterType.IsClass || formatterType.IsAbstract)
                throw new ArgumentException("Invalid formatter.");

            if (this.OrderedFormatters.Any(p => p.Value.Equals(formatterType))) return;

            this.OrderedFormatters.Add(order, formatterType);
        }

        public OptimizationOptions UseOrderer<TOrderer>() where TOrderer : class, IAssetOrderer
        {
            return this.UseOrderer<TOrderer, object>(null);
        }

        public OptimizationOptions UseOrderer<TOrderer, TOptions>(TOptions options) where TOrderer : class, IAssetOrderer
        {
            this.Orderer = typeof(TOrderer);
            this.OrdererOptions = options;
            return this;
        }

        public OptimizationOptions UseFileOrderer(string filePath)
        {
            return this.UseOrderer<FileAssetOrderer, string>(filePath);
        }
    }
}