using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Quantum;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace HnSF
{
    [System.Serializable]
#if HNSF_DISABLE_CONTENT_ASSET_MENU
#else
    [CreateAssetMenu(menuName = "HnSF/Addressables/Content/Fighter Definition")]
#endif
    public partial class AddressablesFighterDefinition : IFighterDefinition
    {
        public override string Name => fighterName;
        public override string Description => description;
        public override bool Selectable => selectable;
        public override int Health => health;

        [SerializeField] public string fighterName;
        [SerializeField, TextArea] public string description;
        [SerializeField] public AssetReferenceT<GameObject> menuVisualReference;
        [SerializeField] public AssetReferenceT<GameObject> fighterReference;
        [SerializeField] public AssetReferenceT<BattleActorDefinition> quantumDefinition;
        [SerializeField] public string[] labelsForLoading;
        [SerializeField] public bool selectable = true;
        [SerializeField] public int health = 10000;
        [SerializeField] public ModAssetSoftReferenceParam[] hudReferences;
        [SerializeField] public TaggedModAssetSoftReference[] hudOverrideReferences;
        [SerializeField] public AssetReferenceT<BaseCommandListDefinition> commandList;

        [NonSerialized] public AsyncOperationHandle<GameObject> fighterHandle;
        [NonSerialized] public AsyncOperationHandle<GameObject> menuVisualHandle;
        [NonSerialized] public AsyncOperationHandle<BattleActorDefinition> quantumDefinitionHandle;
        [NonSerialized] public AsyncOperationHandle<IList<Object>> contentsHandle;
        [NonSerialized] public AsyncOperationHandle<BaseCommandListDefinition> commandListHandle;

        public override async UniTask<bool> Load(string id)
        {
            await base.Load(id);

            if (commandList.IsValid())
            {
                commandListHandle = Addressables.LoadAssetAsync<BaseCommandListDefinition>(commandList);
                await commandListHandle;
                if (commandListHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"Couldn't load command list definition for fighter {id}.");
                    Addressables.Release(commandListHandle);
                    commandListHandle = default;
                    return false;
                }
            }

            return true;
        }

        public override async UniTask<bool> LoadAssets()
        {
            if (labelsForLoading != null && labelsForLoading.Length > 0)
            {
                try
                {
                    if (!contentsHandle.IsValid())
                        contentsHandle = Addressables.LoadAssetsAsync<Object>(
                            labelsForLoading,
                            addressable => { },
                            Addressables.MergeMode.Union,
                            true);
                    await contentsHandle;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Exception thrown while loading fighter {fighterName} contents: {e}");
                    return false;
                }
            }

            if (!fighterHandle.IsValid()) fighterHandle = Addressables.LoadAssetAsync<GameObject>(fighterReference);
            await fighterHandle;

            try
            {
                if (!quantumDefinitionHandle.IsValid())
                    quantumDefinitionHandle = Addressables.LoadAssetAsync<BattleActorDefinition>(quantumDefinition);
                await quantumDefinitionHandle;
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception thrown while loading fighter {fighterName} quantum definition: {e}");
                return false;
            }

            return true;
        }

        public override async UniTask<bool> LoadVisualRepresentation()
        {
            try
            {
                if (!menuVisualHandle.IsValid())
                    menuVisualHandle = Addressables.LoadAssetAsync<GameObject>(menuVisualReference);
                await menuVisualHandle;
                if (menuVisualHandle.Status == AsyncOperationStatus.Succeeded) return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading visual representation of {fighterName}: {e}");
            }

            return false;
        }

        public override GameObject GetVisualRepresentation()
        {
            return menuVisualHandle.Result;
        }

        public override void UnloadVisualRepresentation()
        {
            if (menuVisualHandle.IsValid() && menuVisualHandle.Status == AsyncOperationStatus.Succeeded)
                Addressables.Release(menuVisualHandle);
            menuVisualHandle = default;
        }

        public override GameObject GetFighter()
        {
            return fighterHandle.Result;
        }

        public override BattleActorDefinition GetFighterQuantum()
        {
            return quantumDefinitionHandle.Result;
        }

        public override ModAssetSoftReferenceParam[] GetHUDReferences()
        {
            return hudReferences.ToArray();
        }

        public override TaggedModAssetSoftReference[] GetOverrideHUDReferences()
        {
            return hudOverrideReferences.ToArray();
        }

        public override BaseCommandListDefinition GetCommandList()
        {
            return commandListHandle.Result;
        }

        public override void UnloadAssets()
        {
            if (quantumDefinitionHandle.IsValid())
                Addressables.Release(quantumDefinitionHandle);
            quantumDefinitionHandle = default;

            if (fighterHandle.IsValid())
                Addressables.Release(fighterHandle);
            fighterHandle = default;

            if (contentsHandle.IsValid())
                Addressables.Release(contentsHandle);
            contentsHandle = default;
        }

        public override void Unload()
        {
            base.Unload();
            
            if (commandListHandle.IsValid())
                Addressables.Release(commandListHandle);
            commandListHandle = default;
        }
    }
}
