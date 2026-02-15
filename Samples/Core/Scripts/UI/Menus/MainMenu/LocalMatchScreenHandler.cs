using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HnSF.sessionhandling.handlers;
using Quantum;
using UnityEngine;

namespace HnSF.ui.menus.examples.mainmenu
{
    public class LocalMatchScreenHandler : MenuHandlerBase
    {
        public MainMenuScreenHandler MainMenuScreenHandler;
        [NonSerialized] public GenericContentPickerInstance screenContentPicking;
        [NonSerialized] public GenericScreenGamemodeConfig screenGamemodeConfig;
        public GenericScreenCharacterSelect screenCharacterSelect;
        
        public LoadedAssetHandleWrapper gamemodeAssetHandle;
        public string gamemodeConfiguration;
        List<List<LoadedAssetHandleWrapper>> selectedCharacters = new List<List<LoadedAssetHandleWrapper>>();
        private List<TeamBitmask> selectedTeams = new List<TeamBitmask>();
        public LoadedAssetHandleWrapper mapAssetHandle;
        public bool gamemodeConfigurationEnabled = true;
        
        [Header("Prefabs")]
        public GenericScreenGamemodeConfig gamemodeConfigScreenPrefab;
        public SessionHandlerLocalMatch localMatchSessionHandlerPrefab;
        
        public virtual void Open()
        {
            gameObject.SetActive(true);
            SetupGamemodePickScreen();
            Forward(screenContentPicking);
        }

        public virtual void Close()
        {
            gameObject.SetActive(false);
        }

        protected virtual void OnDestroy()
        {
            ReleaseAssetHandles();
        }

        protected virtual void ReleaseAssetHandles()
        {
            var gameManager = HnSFManagersContainer.instance;
            if (gameManager == null) return;
            
            if(gamemodeAssetHandle.IsValid()) gameManager.contentManager.ReleaseAssetFromMod(gamemodeAssetHandle);
            if(mapAssetHandle.IsValid()) gameManager.contentManager.ReleaseAssetFromMod(mapAssetHandle);

            for (int i = 0; i < selectedCharacters.Count; i++)
            {
                for (int j = 0; j < selectedCharacters[i].Count; j++)
                {
                    gameManager.contentManager.ReleaseAssetFromMod(selectedCharacters[i][j]);
                }
            }
            selectedCharacters.Clear();
            gamemodeAssetHandle = default;
            mapAssetHandle = default;
        }


        // Gamemode Selection
        private void SetupGamemodePickScreen()
        {
            var gameManager = HnSFManagersContainer.instance;
            if(screenContentPicking == null) screenContentPicking = GenericContentPickerInstanceManager.instance.CreateInstance<BaseGamemodeDefinition>(transform);
            screenContentPicking.onContentPicked.AddListener(OnGamemodePicked);
            screenContentPicking.onCancel.AddListener(OnGamemodePickCanceled);
            screenContentPicking.Initialize<BaseGamemodeDefinition>(gameManager.inputManager.GetPlayer(1));
        }
        
        private void OnGamemodePickCanceled(GenericContentPickerInstance arg0)
        {
            
        }

        public bool resolving;
        private void OnGamemodePicked(GenericContentPickerInstance arg0)
        {
            screenContentPicking.onContentPicked.RemoveListener(OnGamemodePicked);
            screenContentPicking.onContentPicked.RemoveListener(OnGamemodePickCanceled);
            
            if (gamemodeAssetHandle.IsValid())
            {
                HnSFManagersContainer.instance.contentManager.ReleaseAssetFromMod(gamemodeAssetHandle);
            }
            gamemodeAssetHandle = screenContentPicking.ConfirmWantedContentAndRemoveFromList();
            screenContentPicking.Uninitialize();
            
            if (gamemodeConfigurationEnabled == false)
            {
                _ = TryQuickConfig();
                return;
            }
            SetupConfigurationScreen();
            Forward(screenGamemodeConfig);
            resolving = false;
        }
        
        private async UniTaskVoid TryQuickConfig()
        {
            SetupConfigurationScreen();
            screenGamemodeConfig.gameObject.SetActive(false);
            await UniTask.WaitUntil(() => screenGamemodeConfig.initialized == true);
            screenGamemodeConfig.OnConfigurationCanceled.RemoveAllListeners();
            screenGamemodeConfig.OnConfigurationConfirmed.RemoveAllListeners();
            var gotQuickSettings = screenGamemodeConfig.ApplySettingsAndSave();
            gamemodeConfiguration = gotQuickSettings;
            Forward(screenCharacterSelect);
            SetupCharacterSelect();
        }

        // Gamemode Configuration
        private void SetupConfigurationScreen()
        {
            if(screenGamemodeConfig == null) screenGamemodeConfig = GameObject.Instantiate(gamemodeConfigScreenPrefab, transform, false);
            screenGamemodeConfig.OnConfigurationCanceled.AddListener(WhenConfigurationCanceled);
            screenGamemodeConfig.OnConfigurationConfirmed.AddListener(WhenConfigurationConfirmed);
            _ = screenGamemodeConfig.Initialize(gamemodeAssetHandle.assetReference);
        }

        private void WhenConfigurationCanceled()
        {
            screenGamemodeConfig.OnConfigurationCanceled.RemoveListener(WhenConfigurationCanceled);
            screenGamemodeConfig.OnConfigurationConfirmed.RemoveListener(WhenConfigurationConfirmed);
            Back();
            SetupGamemodePickScreen();
        }
        
        private void WhenConfigurationConfirmed(string configurationAsJson)
        {
            screenGamemodeConfig.OnConfigurationCanceled.RemoveListener(WhenConfigurationCanceled);
            screenGamemodeConfig.OnConfigurationConfirmed.RemoveListener(WhenConfigurationConfirmed);
            gamemodeConfiguration = configurationAsJson;

            Forward(screenCharacterSelect);
            SetupCharacterSelect();
        }

        // Character Select
        private void SetupCharacterSelect()
        {
            var gameManager = HnSFManagersContainer.instance;
            screenCharacterSelect.Initialize(gameManager.inputManager.GetPlayers());
            screenCharacterSelect.OnConfirmCharacters.AddListener(WhenCharactersConfirmed);
            screenCharacterSelect.OnCancel.AddListener(WhenCharactersCanceled);
        }

        private void WhenCharactersCanceled()
        {
            screenCharacterSelect.OnConfirmCharacters.RemoveListener(WhenCharactersConfirmed);
            screenCharacterSelect.OnCancel.RemoveListener(WhenCharactersCanceled);
            Back();
            
            if (gamemodeConfigurationEnabled == false)
            {
                SetupGamemodePickScreen();
                return;
            }
            SetupConfigurationScreen();
        }
        
        private async void WhenCharactersConfirmed()
        {
            screenCharacterSelect.OnConfirmCharacters.RemoveListener(WhenCharactersConfirmed);
            screenCharacterSelect.OnCancel.RemoveListener(WhenCharactersCanceled);

            var charactersPicked = screenCharacterSelect.GetCharactersPicked();
            screenCharacterSelect.Teardown();

            selectedCharacters.Clear();

            var contentManager = HnSFManagersContainer.instance.contentManager;

            var successfulLoad = true;
            for (int i = 0; i < charactersPicked.Count; i++)
            {
                selectedCharacters.Add(new List<LoadedAssetHandleWrapper>());
                for (int w = 0; w < charactersPicked[i].Count; w++)
                {
                    var loadResults = await contentManager.LoadAssetFromModAsync(charactersPicked[i][w]);
                    if (loadResults.result == false)
                    {
                        successfulLoad = false;
                        break;
                    }
                    selectedCharacters[i].Add(loadResults.handle);
                }

                if (successfulLoad == false) break;
            }

            if (successfulLoad == false)
            {
                Back(); // TODO: Hook the events back up
                return;
            }
            
            SetupMapPick();
            Forward(screenContentPicking);
        }

        // Map Picking
        private void SetupMapPick()
        {
            var gameManager = HnSFManagersContainer.instance;
            screenContentPicking.onContentPicked.AddListener(OnMapPicked);
            screenContentPicking.onCancel.AddListener(OnMapPickCanceled);
            screenContentPicking.Initialize<IMapDefinition>(gameManager.inputManager.GetPlayer(1));
        }

        private void TeardownMapPick()
        {
            screenContentPicking.onContentPicked.RemoveListener(OnMapPicked);
            screenContentPicking.onContentPicked.RemoveListener(OnMapPickCanceled);
        }
        
        private void OnMapPickCanceled(GenericContentPickerInstance arg0)
        {
            TeardownMapPick();
            Back();
            SetupCharacterSelect();
        }

        private async void OnMapPicked(GenericContentPickerInstance arg0)
        {
            TeardownMapPick();
            
            if (mapAssetHandle.IsValid())
            {
                HnSFManagersContainer.instance.contentManager.ReleaseAssetFromMod(mapAssetHandle);
            }
            mapAssetHandle = screenContentPicking.ConfirmWantedContentAndRemoveFromList();
            screenContentPicking.Uninitialize();
            
            var gameManager = HnSFManagersContainer.instance;

            selectedTeams = new List<TeamBitmask>();
            for (int i = 0; i < selectedCharacters.Count; i++)
                selectedTeams.Add((TeamBitmask)(1 << i));
            
            var localMatchSessionHandler = gameManager.sessionHandlerManager.CreateSessionHandler("LocalMatch", localMatchSessionHandlerPrefab);
            if (localMatchSessionHandler == null) return;
            var contentBundles = await PlayerMatchContentBundle.TryBuildPlayerContentBundles(selectedCharacters, selectedTeams);
            (localMatchSessionHandler as SessionHandlerLocalMatch).gamemodeConfiguration = gamemodeConfiguration;
            BeforeLocalMatchHandlerStart(localMatchSessionHandler, contentBundles);
            _ = (localMatchSessionHandler as SessionHandlerLocalMatch).PrepareForMatchAndStart(new QuantumMatchContentBundle()
            {
                gamemodeReference = gamemodeAssetHandle.assetReference,
                mapReference = mapAssetHandle.assetReference,
                localPlayerBundles = contentBundles,
                playerCount = selectedCharacters.Count
            });
        }

        protected virtual void BeforeLocalMatchHandlerStart(SessionHandlerLocalMatch localMatchSessionHandler,
            List<PlayerMatchContentBundle> contentBundles)
        {
            
        }
    }
}