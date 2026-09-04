using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Quantum;
using UnityEngine;

namespace HnSF
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "HnSF/UMod/Content/Gamemode Definition")]
    public partial class UModGamemodeDefinition : BaseGamemodeDefinition
    {
        public override string Name => gamemodeName;
        public override string Description => description;

        [SerializeField] private string gamemodeName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private ExternalModAssetSoftReference gamemodeMatchHandler;
        [SerializeField] private ExternalModAssetSoftReference quantumAsset;
        [SerializeField] private ExternalModAssetSoftReference[] contentReferencesForLoading;
        [SerializeField] private string[] contentFoldersForLoading;
        [SerializeField] private GamemodeTeamRule[] teamRules;
        [SerializeField] private GamemodeTeamConfig[] defaultTeamConfig;
        [field: SerializeField] public override int MinimumPlayers { get; protected set; } = 1;
        [field: SerializeField] public override int MaximumPlayers { get; protected set; } = 16;
        
        [NonSerialized] private LoadedAssetHandleWrapper gamemodeMatchHandlerHandle;
        [NonSerialized] private LoadedAssetHandleWrapper quantumAssetHandle;
        [NonSerialized] private List<LoadedAssetHandleWrapper> contentsHandle;

        public override GamemodeTeamRule[] GetTeamRules()
        {
            return teamRules;
        }

        public override GamemodeTeamConfig[] GetDefaultTeamConfig()
        {
            return defaultTeamConfig;
        }

        public override GamemodeSettingsBase GetDefaultGamemodeSettings()
        {
            return null;
        }

        public override GamemodeSettingsBase GetGamemodeSettingsInstance()
        {
            return null;
        }

        public override async UniTask<bool> LoadAssets()
        {
            var contentManager = HnSFManagersContainer.instance.contentManager;

            var modHost = (modDefinition as UModLoadedModDefinition).modHost;
            var modInfo = (modDefinition.modAsset as UModModInfoAsset);
            
            try
            {
                foreach (var cref in contentReferencesForLoading)
                {
                    var crefLoadResult = await contentManager.LoadAssetFromModAsync(cref.reference);
                    if (crefLoadResult == null)
                        throw new Exception($"Failed to load content reference. {cref.reference.ToString()}");
                    contentsHandle.Add(crefLoadResult);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception thrown while loading gamemode {gamemodeName} contents: {e}");
                return false;
            }

            try
            {
                var quantumAssetLoadResult = await contentManager.LoadAssetFromModAsync(quantumAsset.reference);
                if (quantumAssetLoadResult == null)
                    throw new Exception($"Failed to load fighter. {quantumAsset.reference.ToString()}");
                quantumAssetHandle = quantumAssetLoadResult;
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception thrown while loading gamemode {gamemodeName} quantum definition: {e}");
                return false;
            }

            try
            {
                var matchHandlerLoadResult =
                    await contentManager.LoadAssetFromModAsync(gamemodeMatchHandler.reference);
                if (matchHandlerLoadResult == null)
                    throw new Exception($"Failed to load fighter. {gamemodeMatchHandler.reference.ToString()}");
                gamemodeMatchHandlerHandle = matchHandlerLoadResult;
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception thrown while loading gamemode {gamemodeName}: {e}");
                return false;
            }

            return true;
        }
        public override GameObject GetMatchHandler()
        {
            return gamemodeMatchHandlerHandle.GetAsset<GameObject>();
        }

        public override void UnloadAssets()
        {
        }
    }
}