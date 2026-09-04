using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Quantum;

namespace HnSF
{
    [Serializable]
    public partial class PlayerMatchContentBundle
    {
        public TeamBitmask requestedTeam;
        public LoadedAssetHandleWrapper[] fighterAssetRefs;
        
        public static async UniTask<List<PlayerMatchContentBundle>> TryBuildPlayerContentBundles(List<List<LoadedAssetHandleWrapper>> selectedCharacters, List<TeamBitmask> selectedTeams)
        {
            if (selectedCharacters.Count != selectedTeams.Count) return null;
            
            var l = new List<PlayerMatchContentBundle>();

            bool valid = true;
            for (int i = 0; i < selectedCharacters.Count; i++)
            {
                PlayerMatchContentBundle bundle = new PlayerMatchContentBundle();
                var createResult = await bundle.Create(selectedCharacters[i], selectedTeams[i]);
                if (createResult == false)
                {
                    valid = false;
                    break;
                }
                l.Add(bundle);
            }

            if (valid)
            {
                return l;
            }
            
            for(int i = 0; i < l.Count; i++) l[i].Teardown();
            return null;
        }
        
        public virtual async UniTask<bool> Create(string[] fighterReferences, TeamBitmask teamId)
        {
            requestedTeam = teamId;

            fighterAssetRefs = new LoadedAssetHandleWrapper[fighterReferences.Length];
            for (int i = 0; i < fighterReferences.Length; i++)
            {
                var loadedAssetHandle = await HnSFManagersContainer.instance.contentManager.LoadAssetFromModAsync(new ModAssetSoftReference(fighterReferences[i]));
                if (loadedAssetHandle == null) return false;
                fighterAssetRefs[i] = loadedAssetHandle;
                await loadedAssetHandle.GetAsset<IFighterDefinition>().LoadAssets();
            }
            
            return true;
        }
        
        public virtual async UniTask<bool> Create(ModAssetSoftReference[] fighterReferences, TeamBitmask teamId)
        {
            requestedTeam = teamId;

            fighterAssetRefs = new LoadedAssetHandleWrapper[fighterReferences.Length];
            for (int i = 0; i < fighterReferences.Length; i++)
            {
                var loadedAssetHandle = await HnSFManagersContainer.instance.contentManager.LoadAssetFromModAsync(fighterReferences[i]);
                if (loadedAssetHandle == null) return false;
                fighterAssetRefs[i] = loadedAssetHandle;
                await loadedAssetHandle.GetAsset<IFighterDefinition>().LoadAssets();
            }
            
            return true;
        }
        
        public virtual async UniTask<bool> Create(List<LoadedAssetHandleWrapper> fighterReferences, TeamBitmask teamId)
        {
            requestedTeam = teamId;

            fighterAssetRefs = new LoadedAssetHandleWrapper[fighterReferences.Count];
            for (int i = 0; i < fighterReferences.Count; i++)
            {
                var loadedAssetHandle =
                    await HnSFManagersContainer.instance.contentManager.LoadAssetFromModAsync(fighterReferences[i].AssetReference);
                if (loadedAssetHandle == null) return false;
                fighterAssetRefs[i] = loadedAssetHandle;
                await fighterAssetRefs[i].GetAsset<IFighterDefinition>().LoadAssets();
            }
            return true;
        }

        public virtual void Teardown()
        {
            for (int i = 0; i < fighterAssetRefs.Length; i++)
            {
                fighterAssetRefs[i]?.Dispose();
                fighterAssetRefs[i] = null;
            }
        }

        public AssetRef<BattleActorDefinition>[] GetQuantumFighterDefinitionAssetRefs()
        {
            var quantumFighterRefs = new List<AssetRef<BattleActorDefinition>>();

            for (int i = 0; i < fighterAssetRefs.Length; i++)
            {
                quantumFighterRefs.Add(fighterAssetRefs[i].GetAsset<IFighterDefinition>().GetFighterQuantum());
            }
            
            return quantumFighterRefs.ToArray();
        }
    }
}

