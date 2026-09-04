using System;
using HnSF;
#if HNSF_UMOD
using UMod;
#endif
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

public sealed class LoadedAssetHandleWrapper : IDisposable
{
    private readonly ILoadedAssetHandleOwner _owner;
    private bool _isReleased;

    public AssetHandleType HandleType { get; }
    public ModAssetSoftReference AssetReference { get; }

    internal AsyncOperationHandle AddressablesHandle { get; }

#if HNSF_UMOD
    internal ModAsyncOperation UModHandle { get; }
#endif

    public bool IsReleased => _isReleased;

    public bool IsValid
    {
        get
        {
            if (_isReleased)
                return false;

            return HandleType switch
            {
                AssetHandleType.Addressables =>
                    AddressablesHandle.IsValid(),
#if HNSF_UMOD
                AssetHandleType.UMod =>
                    UModHandle != null,
#endif
                _ => false
            };
        }
    }

    public LoadedAssetHandleWrapper(
        ILoadedAssetHandleOwner owner,
        ModAssetSoftReference assetReference,
        AsyncOperationHandle addressablesHandle)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        HandleType = AssetHandleType.Addressables;
        AssetReference = assetReference;
        AddressablesHandle = addressablesHandle;

#if HNSF_UMOD
        UModHandle = null;
#endif
    }

#if HNSF_UMOD
    public LoadedAssetHandleWrapper(
        ILoadedAssetHandleOwner owner,
        ModAssetSoftReference assetReference,
        ModAsyncOperation umodHandle)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        HandleType = AssetHandleType.UMod;
        AssetReference = assetReference;
        UModHandle = umodHandle;
        AddressablesHandle = default;
    }
#endif

    public Object GetAsset()
    {
        if (!IsValid)
            return null;

        return HandleType switch
        {
            AssetHandleType.Addressables =>
                AddressablesHandle.Result as Object,
#if HNSF_UMOD
            AssetHandleType.UMod =>
                UModHandle.Result as Object,
#endif
            _ => null
        };
    }

    public T GetAsset<T>() where T : Object
    {
        return GetAsset() as T;
    }

    public bool TryGetAsset<T>(out T asset) where T : Object
    {
        asset = GetAsset<T>();
        return asset != null;
    }

    public void Release()
    {
        Dispose();
    }

    public void Dispose()
    {
        if (_isReleased)
            return;

        _isReleased = true;
        _owner.Release(this);
    }

    public override string ToString()
    {
        return $"{AssetReference} ({HandleType}, {(_isReleased ? "released" : "active")})";
    }
}