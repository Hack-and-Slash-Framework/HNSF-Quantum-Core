using HnSF.core.state;
using UnityEditor;
using UnityEngine;

namespace HnSF
{
    public class StateUpdater : EditorWindow
    {
        [MenuItem("Tools/HnSF/Update States")]
        public static void UpdateStates()
        {
            var assetGuids = AssetDatabase.FindAssets($"t:{nameof(HNSFState)}");

            foreach (var guid in assetGuids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<HNSFState>(assetPath);
                if(asset == null)
                    continue;
                
                asset.RebuildGroups();
            }

            Debug.Log($"Updated {assetGuids.Length} states.");
        }
    }
}