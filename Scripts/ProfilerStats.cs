using Unity.Profiling;

namespace HnSF
{
    public static class ProfilerStats
    {
        public static readonly ProfilerCategory LoadedAssetsCategory = ProfilerCategory.Scripts;

        public const string LoadedAssetsCountName = "Assets Loaded";

        public static readonly ProfilerCounterValue<int> LoadedAssetsCount =
            new ProfilerCounterValue<int>(LoadedAssetsCategory, LoadedAssetsCountName, ProfilerMarkerDataUnit.Count,
                ProfilerCounterOptions.None);
    }
}