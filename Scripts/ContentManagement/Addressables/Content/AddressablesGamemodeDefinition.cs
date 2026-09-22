using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Quantum;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Scripting.APIUpdating;
using Object = UnityEngine.Object;

namespace HnSF
{
    [System.Serializable]
#if HNSF_DISABLE_CONTENT_ASSET_MENU
#else
    [CreateAssetMenu(menuName = "HnSF/Addressables/Content/Gamemode Definition")]
#endif
    public partial class AddressablesGamemodeDefinition : BaseGamemodeDefinition
    {
        public override string Name => gamemodeName;
        public override string Description => description;
        public override bool Selectable => selectable;

        [SerializeField] public bool selectable = true;
        [SerializeField] private string gamemodeName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private AssetReferenceT<GameObject> gamemodeMatchHandler;
        [SerializeField] private string[] labelsForLoading;
        [SerializeField] private AssetReferenceT<GamemodeSettingsBase> gamemodeSettingsReference;
        [SerializeField] private GamemodeTeamRule[] teamRules;
        [SerializeField] private GamemodeTeamConfig[] defaultTeamConfig;
        [field: SerializeField] public override int MinimumPlayers { get; protected set; } = 1;
        [field: SerializeField] public override int MaximumPlayers { get; protected set; } = 16;

        [NonSerialized] private AsyncOperationHandle<GameObject> gamemodeMatchHandlerHandle;
        [NonSerialized] private AsyncOperationHandle<IList<Object>> contentsHandle;
        [NonSerialized] private AsyncOperationHandle<GamemodeSettingsBase> gamemodeSettingsHandle;

        public override GamemodeTeamRule[] GetTeamRules()
        {
            return teamRules;
        }

        public override GamemodeTeamConfig[] GetDefaultTeamConfig()
        {
            return defaultTeamConfig;
        }

        protected override async UniTask LoadAssetsInternal()
        {
            try
            {
                if (labelsForLoading != null && labelsForLoading.Length > 0)
                {
                    contentsHandle = Addressables.LoadAssetsAsync<Object>(
                            labelsForLoading,
                            addressable => { },
                            Addressables.MergeMode.Union,
                            true);
                    await contentsHandle;
                }

                gamemodeMatchHandlerHandle = Addressables.LoadAssetAsync<GameObject>(gamemodeMatchHandler);
                await gamemodeMatchHandlerHandle;

                gamemodeSettingsHandle = Addressables.LoadAssetAsync<GamemodeSettingsBase>(gamemodeSettingsReference);
                await gamemodeSettingsHandle;
                
                ReportAssetsLoadResult(true);
            }
            catch (Exception e)
            {
                ReportAssetsLoadResult(false);
            }
        }

        public override GameObject GetMatchHandler()
        {
            return gamemodeMatchHandlerHandle.Result;
        }

        public override GamemodeSettingsBase GetDefaultGamemodeSettings()
        {
            if (!gamemodeSettingsHandle.IsValid() ||
                gamemodeSettingsHandle.Status != AsyncOperationStatus.Succeeded) return null;
            return gamemodeSettingsHandle.Result;
        }

        public override GamemodeSettingsBase GetGamemodeSettingsInstance()
        {
            if (!gamemodeSettingsHandle.IsValid() ||
                gamemodeSettingsHandle.Status != AsyncOperationStatus.Succeeded) return null;
            return gamemodeSettingsHandle.Result.GetInstance();
        }

        public override void UnloadAssets()
        {
            if (gamemodeMatchHandlerHandle.IsValid())
                Addressables.Release(gamemodeMatchHandlerHandle);
            gamemodeMatchHandlerHandle = default;

            if (contentsHandle.IsValid())
                Addressables.Release(contentsHandle);
            contentsHandle = default;

            if (gamemodeSettingsHandle.IsValid())
                Addressables.Release(gamemodeSettingsHandle);
            gamemodeSettingsHandle = default;
        }
    }
}