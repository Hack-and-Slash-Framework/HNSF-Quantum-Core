using System;
using Cysharp.Threading.Tasks;
using Quantum;
using UMod;
using UnityEngine;

namespace HnSF
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "HnSF/UMod/Content/Hud Element Definition")]
    public partial class UModHudElementDefinition : BaseHudElementDefinition
    {
        public override string Name => label;
        public override string Description => description;
        
        public override AssetRef<Tag> ElementParent => elementParent;
        
        [SerializeField] protected string label;
        [SerializeField, TextArea] protected string description;
        [SerializeField] protected ExternalModAssetSoftReference hudElementReference;
        [SerializeField] protected AssetRef<Tag> elementParent;
        
        [NonSerialized] protected ModAsyncOperation assetHandle;

        public override async UniTask<bool> LoadAssets()
        {
            if (assetHandle.IsDone && assetHandle.IsSuccessful) return true;
            
            var contentManager = HnSFManagersContainer.instance.contentManager;

            var modHost = (modDefinition as UModLoadedModDefinition).modHost;
            var modInfo = (modDefinition.modAsset as UModModInfoAsset);
            
            try
            {
                if(assetHandle.Status == string.Empty) assetHandle = modHost.Assets.LoadAsync(modInfo.ConvertIDToAssetPath(hudElementReference.reference.assetID));
                await assetHandle;
                if(!assetHandle.Result) throw new Exception($"Failed to load Hud Element Gameobject. {hudElementReference.reference.ToString()}");
                
            }
            catch (Exception e)
            {
                assetHandle = default;
                Debug.LogError($"Error loading HUD Element {label} ({name}). {e}");
                return false;
            }
            
            return true;
        }

        public override HudElementContainer GetElementContainer()
        {
            return (assetHandle.Result as GameObject).GetComponent<HudElementContainer>();
        }

        public override GameObject GetElementPrefab()
        {
            return (assetHandle.Result as GameObject);
        }

        public override void UnloadAssets()
        {
            
        }

        public override void Unload()
        {
        }
    }
}