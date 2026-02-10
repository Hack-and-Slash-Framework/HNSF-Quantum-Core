using Unity.Profiling;
using Unity.Profiling.Editor;
using UnityEngine;

namespace HnSF
{
    [System.Serializable]
    [ProfilerModuleMetadata("Loaded Assets")] 
    public class LoadedAssetsProfileModule : ProfilerModule
    {
        static readonly ProfilerCounterDescriptor[] k_Counters = new ProfilerCounterDescriptor[]
        {
            new ProfilerCounterDescriptor(ProfilerStats.LoadedAssetsCountName, ProfilerStats.LoadedAssetsCategory),
        };

        public LoadedAssetsProfileModule() : base(k_Counters)
        {
            
        }

        public override ProfilerModuleViewController CreateDetailsViewController()
        {
            return new LoadedAssetsViewController(ProfilerWindow);
        }
    }
}