using System;
using HnSF;
#if HNSF_UMOD
using UMod;
#endif
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

[System.Serializable]
public struct LoadedAssetHandleWrapper : IEquatable<LoadedAssetHandleWrapper>
{
    public AssetHandleType handleType;
    public ModAssetSoftReference assetReference;
    // Addressables
    public AsyncOperationHandle addressablesHandle;
#if HNSF_UMOD
    // UMod
    public ModAsyncOperation umodHandle;
#endif

    public T GetAsset<T>() where T : Object
    {
        switch (handleType)
        {
            case AssetHandleType.Addressables:
                return addressablesHandle.Result as T;
#if HNSF_UMOD
            case AssetHandleType.UMod:
                return umodHandle.Result as T;
#endif
        }
        return null;
    }

    public void Release()
    {
        if (HnSFManagersContainer.instance == null || handleType == AssetHandleType.None) return;
        HnSFManagersContainer.instance.contentManager.ReleaseAssetFromMod(this);
        handleType = AssetHandleType.None;
        assetReference = default;
        addressablesHandle = default;
#if HNSF_UMOD
        umodHandle = default;
#endif
    }

    public void Teardown(bool releaseAsset = true)
    {
        if (HnSFManagersContainer.instance == null || handleType == AssetHandleType.None) return;
        if(releaseAsset) HnSFManagersContainer.instance.contentManager.ReleaseAssetFromMod(this);
        handleType = AssetHandleType.None;
        assetReference = default;
        addressablesHandle = default;
#if HNSF_UMOD
        umodHandle = default;
#endif
    }

    public bool IsValid()
    {
        return handleType != AssetHandleType.None;
    }

    public bool Equals(LoadedAssetHandleWrapper other)
    {
        return handleType == other.handleType && assetReference == other.assetReference && addressablesHandle.Equals(other.addressablesHandle) 
#if HNSF_UMOD
               && Equals(umodHandle, other.umodHandle);
#else
               ;
#endif
    }

    public override bool Equals(object obj)
    {
        return obj is LoadedAssetHandleWrapper other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)handleType, assetReference, addressablesHandle
#if HNSF_UMOD
            , umodHandle);
#else
            );
#endif
    }

    public override string ToString()
    {
        return $"{assetReference.ToString()} ({handleType.ToString()})";
    }
}