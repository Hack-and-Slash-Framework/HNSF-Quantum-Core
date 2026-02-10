using Unity.Profiling.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HnSF
{
    public class LoadedAssetsViewController : ProfilerModuleViewController
    {
        public LoadedAssetsViewController(ProfilerWindow profilerWindow) : base(profilerWindow)
        {
        }

        protected override VisualElement CreateView()
        {
            var view = new VisualElement();
            return view;
        }
    }
}