using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HnSF
{
    [CreateAssetMenu(menuName = "HnSF/Addressables/Content/Fighter Definitions Container",
        fileName = "fighterdefinitioncontainer")]
    public class AddressableFighterDefinitionContainer : BaseFighterDefinitionContainer
    {
        [SerializeField] private AssetReferenceT<AddressablesFighterDefinition>[] fighterReferences;

        [NonSerialized] private LoadedAssetHandleWrapper[] definitionHandles = null;

        public override UniTask<bool> Load(string id)
        {
            base.Load(id);
            definitionHandles = new LoadedAssetHandleWrapper[fighterReferences.Length];
            return new UniTask<bool>(true);
        }

        public override async UniTask<bool> LoadAssets()
        {
            var modAsset = modDefinition.modAsset as AddressablesModInfoAsset;

            try
            {
                for (var i = 0; i < fighterReferences.Length; i++)
                {
                    var id = await GeneralHelpers.GetAssetRefID(fighterReferences[i]);
                    if (string.IsNullOrEmpty(id))
                    {
                        Debug.LogError($"Couldn't get asset ID for {fighterReferences[i]} at index {i}.");
                        continue;
                    }

                    var loadResult = await modAsset.LoadAssetByIDAsync(id);
                    if (!loadResult.result)
                    {
                        Debug.LogError($"Couldn't load asset ID {id} at index {i}. ({name})");
                        continue;
                    }

                    definitionHandles[i] = loadResult.handle;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in FighterDefinitionContainer {name} while loading fighters: {e}");
                return false;
            }

            return true;
        }

        public override IFighterDefinition[] GetFighters()
        {
            var fighterList = new List<IFighterDefinition>();
            if (definitionHandles == null) return fighterList.ToArray();

            foreach (var handle in definitionHandles)
            {
                if(!handle.IsValid()) continue;
                fighterList.Add(handle.addressablesHandle.Result as IFighterDefinition);
            }

            return fighterList.ToArray();
        }

        public override void UnloadAssets()
        {
            if (definitionHandles == null) return;

            var modAsset = modDefinition.modAsset as AddressablesModInfoAsset;

            for (int i = 0; i < definitionHandles.Length; i++)
            {
                if(definitionHandles[i].IsValid() == false) continue;
                modAsset.ReleaseAsset(definitionHandles[i]);
            }

            definitionHandles = null;
        }
    }
}
