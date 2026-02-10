#if HNSF_UMOD
namespace Quantum.Editor
{
    partial class QuantumAssetSourceFactoryUMod : IQuantumAssetSourceFactory
    {
        /// <inheritdoc cref="IQuantumAssetSourceFactory.Order"/>
        public const int Order = 2000;

        int IQuantumAssetSourceFactory.Order => Order;

        /// <summary>
        /// Creates <see cref="QuantumAssetSourceUMod{T}"/> if the asset is a UMod asset.
        /// </summary>
        protected bool TryCreateInternal<TSource, TAsset>(in QuantumAssetSourceFactoryContext context, out TSource result) 
            where TSource : QuantumAssetSourceUMod<TAsset>, new()
            where TAsset : UnityEngine.Object
        {
            result = default;
            return false;
            /*
            if (!PathUtils.TryMakeRelativeToFolder(context.AssetPath, "/Resources/", out var resourcePath)) {
                result = default;
                return false;
            }

            var withoutExtension = PathUtils.GetPathWithoutExtension(resourcePath);
            result = new TSource() {
                ResourcePath = withoutExtension,
                SubObjectName = context.IsMainAsset ? string.Empty : context.AssetName,
            };
            return true;*/
        }
        
        public IQuantumAssetObjectSource TryCreateAssetObjectSource(in QuantumAssetSourceFactoryContext context)
        {
            if (TryCreateInternal<QuantumAssetObjectSourceUMod, Quantum.AssetObject>(context, out var result))
            {
                result.SerializableAssetType = context.AssetType;
            }

            return result;
        }
    }
}
#endif