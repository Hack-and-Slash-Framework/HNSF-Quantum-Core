public struct AssetLoadResult
{
    public bool result;
    public LoadedAssetHandleWrapper handle;

    public AssetLoadResult(bool result, LoadedAssetHandleWrapper handle)
    {
        this.result = result;
        this.handle = handle;
    }
}
