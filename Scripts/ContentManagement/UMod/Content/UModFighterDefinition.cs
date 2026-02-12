using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Quantum;
using UMod;
using UnityEngine;

namespace HnSF
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "HnSF/UMod/Content/Fighter Definition")]
    public partial class UModFighterDefinition : IFighterDefinition, IOnUModPrebuild
    {
        public override string Name => fighterName;
        public override string Description => description;
        public override bool Selectable => selectable;
        public override int Health => health;

        [SerializeField] public string fighterName;
        [SerializeField, TextArea] public string description;
        [SerializeField] public ExternalModAssetSoftReference fighterReference;
        [SerializeField, HideInInspector] public ModAssetSoftReference fighterRef;
        [SerializeField] public ExternalModAssetSoftReference fighterQuantumReference;
        [SerializeField, HideInInspector] public ModAssetSoftReference fighterQuantumRef;
        [SerializeField] public ExternalModAssetSoftReference[] contentReferencesForLoading;
        [SerializeField, HideInInspector] public ModAssetSoftReference[] contentRefsForLoading;
        [SerializeField] public bool selectable = true;
        [SerializeField] public int health = 10000;
        [SerializeField] public ModAssetSoftReferenceParam[] hudReferences;
        [SerializeField] public TaggedModAssetSoftReference[] hudOverrideReferences;
        [SerializeField] public ExternalModAssetSoftReference commandList;
        [SerializeField] public ModAssetSoftReference commandListRef;

        //[NonSerialized] public bool loaded;
        [NonSerialized] public ModAsyncOperation fighterHandle;
        [NonSerialized] public ModAsyncOperation quantumDefinitionHandle;
        [NonSerialized] public List<ModAsyncOperation> contentsHandle;
        [NonSerialized] public ModAsyncOperation commandListHandle;

        public virtual void OnUModPrebuild()
        {
            fighterRef = fighterReference ? fighterReference.reference : default;
            fighterQuantumRef = fighterQuantumReference ? fighterQuantumReference.reference : default;

            contentRefsForLoading = new ModAssetSoftReference[contentReferencesForLoading.Length];
            for (int i = 0; i < contentReferencesForLoading.Length; i++)
            {
                contentRefsForLoading[i] = contentReferencesForLoading[i].reference;
            }

            commandListRef = commandList ? commandList.reference : default;
        }

        public override async UniTask<bool> Load(string id)
        {
            await base.Load(id);
            var modAsset = modDefinition.modAsset as UModModInfoAsset;
            var modHost = (modAsset.ModDefinition as UModLoadedModDefinition).modHost;
            contentsHandle = new List<ModAsyncOperation>();

            if (commandList)
            {
                var commandListLoadHandle =
                    modHost.Assets.LoadAsync(modAsset.ConvertIDToAssetPath(commandList.reference.assetID));
                await commandListLoadHandle;
                if (!commandListLoadHandle.IsSuccessful) return false;
                commandListHandle = commandListLoadHandle;
            }

            return true;
        }

        public override async UniTask<bool> LoadAssets()
        {
            var modAsset = modDefinition.modAsset as UModModInfoAsset;
            var modHost = (modAsset.ModDefinition as UModLoadedModDefinition).modHost;

            try
            {
                foreach (var cref in contentRefsForLoading)
                {
                    var crefLoadHandles = modAsset.GetAssetHandlesFromAssetId(cref.assetID);
                    foreach (var crefLoadHandle in crefLoadHandles)
                    {
                        await crefLoadHandle;
                        if (!crefLoadHandle.IsSuccessful) continue;
                        contentsHandle.Add(crefLoadHandle);
                    }
                }

                var fighterLoadHandle = modHost.Assets.LoadAsync(modAsset.ConvertIDToAssetPath(fighterRef.assetID));
                await fighterLoadHandle;
                if (!fighterLoadHandle.IsSuccessful)
                    throw new Exception($"Failed to load fighter. {fighterRef.ToString()}");
                fighterHandle = fighterLoadHandle;

                var fighterQuantumLoadHandle =
                    modHost.Assets.LoadAsync(modAsset.ConvertIDToAssetPath(fighterQuantumRef.assetID));
                ;
                if (!fighterQuantumLoadHandle.IsSuccessful)
                    throw new Exception($"Failed to load quantum fighter. {fighterQuantumRef.ToString()}");
                quantumDefinitionHandle = fighterQuantumLoadHandle;
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception thrown while loading Fighter {fighterName}: {e}");
                return false;
            }

            return true;
        }

        public override UniTask<bool> LoadVisualRepresentation()
        {
            return default;
        }

        public override GameObject GetVisualRepresentation()
        {
            return null;
        }

        public override void UnloadVisualRepresentation()
        {
        }

        public override GameObject GetFighter()
        {
            try
            {
                return fighterHandle.Result as GameObject;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error getting Fighter {fighterName}: {e}");
                return null;
            }
        }

        public override BattleActorDefinition GetFighterQuantum()
        {
            try
            {
                return quantumDefinitionHandle.Result as BattleActorDefinition;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error getting Fighter {fighterName}: {e}");
                return null;
            }
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
            return null;
        }

        public override void UnloadAssets()
        {
            fighterHandle = null;
            quantumDefinitionHandle = null;
            contentsHandle.Clear();
            commandListHandle = null;
        }

        public override void Unload()
        {

        }
    }
}