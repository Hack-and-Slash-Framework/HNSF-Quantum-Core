namespace HnSF
{
    public interface ILoadedAssetHandleOwner
    {
        void Release(LoadedAssetHandleWrapper handle);
    }
}