using System;
using System.Collections.Generic;
using CT.MenuNav;
using Cysharp.Threading.Tasks;
using HnSF.sessionhandling.handlers;
using Quantum;
using UnityEngine;
using UnityEngine.Serialization;

namespace HnSF.ui.menus.examples.mainmenu
{
    public class LocalMatchScreenHelper : MonoBehaviour
    {
        public MainMenuScreenManager screenManager;
        [FormerlySerializedAs("MainMenuScreenHandler")] public MainMenuHelper mainMenuHelper;
        [NonSerialized] public GenericContentPickerInstance pageContentPicking;
        [NonSerialized] public GenericPageGamemodeConfig sectionGamemodeConfig;
        [FormerlySerializedAs("screenCharacterSelect")] public GenericPageCharacterSelect pageCharacterSelect;
        
        public LoadedAssetHandleWrapper gamemodeAssetHandle;
        public string gamemodeConfiguration;
        List<List<LoadedAssetHandleWrapper>> selectedCharacters = new List<List<LoadedAssetHandleWrapper>>();
        private List<TeamBitmask> selectedTeams = new List<TeamBitmask>();
        public LoadedAssetHandleWrapper mapAssetHandle;
        public bool gamemodeConfigurationEnabled = true;
        
        [FormerlySerializedAs("gamemodeConfigScreenPrefab")] [Header("Prefabs")]
        public GenericPageGamemodeConfig gamemodeConfigPagePrefab;
        public SessionHandlerLocalMatch localMatchSessionHandlerPrefab;

        private int playerCount;
        
        public virtual void Open(int playerAmount)
        {
            playerCount = playerAmount;
            gameObject.SetActive(true);
            SetupGamemodePickScreen();
            _ = screenManager.TryForwardPage(pageContentPicking);
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
            if(pageContentPicking == null) pageContentPicking = GenericContentPickerInstanceManager.instance.CreateInstance<BaseGamemodeDefinition>(transform);
            pageContentPicking.onContentPicked.AddListener(OnGamemodePicked);
            pageContentPicking.onCancel.AddListener(OnGamemodePickCanceled);
            pageContentPicking.currentManager = screenManager;
            pageContentPicking.Initialize<BaseGamemodeDefinition>();
        }
        
        private void OnGamemodePickCanceled(GenericContentPickerInstance arg0)
        {
            
        }

        public bool resolving;
        private void OnGamemodePicked(GenericContentPickerInstance arg0)
        {
            pageContentPicking.onContentPicked.RemoveListener(OnGamemodePicked);
            pageContentPicking.onContentPicked.RemoveListener(OnGamemodePickCanceled);
            
            if (gamemodeAssetHandle.IsValid())
            {
                HnSFManagersContainer.instance.contentManager.ReleaseAssetFromMod(gamemodeAssetHandle);
            }
            gamemodeAssetHandle = pageContentPicking.ConfirmWantedContentAndRemoveFromList();
            pageContentPicking.Uninitialize();
            
            if (gamemodeConfigurationEnabled == false)
            {
                _ = TryQuickConfig();
                return;
            }
            SetupConfigurationScreen();
            _ = screenManager.TryForwardPage(sectionGamemodeConfig);
            resolving = false;
        }
        
        private async UniTaskVoid TryQuickConfig()
        {
            SetupConfigurationScreen();
            sectionGamemodeConfig.gameObject.SetActive(false);
            await UniTask.WaitUntil(() => sectionGamemodeConfig.initialized == true);
            sectionGamemodeConfig.OnConfigurationCanceled.RemoveAllListeners();
            sectionGamemodeConfig.OnConfigurationConfirmed.RemoveAllListeners();
            var gotQuickSettings = sectionGamemodeConfig.ApplySettingsAndSave();
            gamemodeConfiguration = gotQuickSettings;
            _ = screenManager.TryForwardPage(pageCharacterSelect);
            SetupCharacterSelect();
        }

        // Gamemode Configuration
        private void SetupConfigurationScreen()
        {
            if(sectionGamemodeConfig == null) sectionGamemodeConfig = GameObject.Instantiate(gamemodeConfigPagePrefab, transform, false);
            sectionGamemodeConfig.OnConfigurationCanceled.AddListener(WhenConfigurationCanceled);
            sectionGamemodeConfig.OnConfigurationConfirmed.AddListener(WhenConfigurationConfirmed);
            _ = sectionGamemodeConfig.Initialize(gamemodeAssetHandle.assetReference);
        }

        private void WhenConfigurationCanceled()
        {
            sectionGamemodeConfig.OnConfigurationCanceled.RemoveListener(WhenConfigurationCanceled);
            sectionGamemodeConfig.OnConfigurationConfirmed.RemoveListener(WhenConfigurationConfirmed);
            _ = screenManager.TryBackPage();
            SetupGamemodePickScreen();
        }
        
        private void WhenConfigurationConfirmed(string configurationAsJson)
        {
            sectionGamemodeConfig.OnConfigurationCanceled.RemoveListener(WhenConfigurationCanceled);
            sectionGamemodeConfig.OnConfigurationConfirmed.RemoveListener(WhenConfigurationConfirmed);
            gamemodeConfiguration = configurationAsJson;
            
            _ = screenManager.TryForwardPage(pageCharacterSelect);
            SetupCharacterSelect();
        }

        // Character Select
        private void SetupCharacterSelect()
        {
            var gameManager = HnSFManagersContainer.instance;
            _ = pageCharacterSelect.Initialize(playerCount);
            pageCharacterSelect.OnConfirmCharacters.AddListener(WhenCharactersConfirmed);
            pageCharacterSelect.OnCancel.AddListener(WhenCharactersCanceled);
        }

        private void WhenCharactersCanceled()
        {
            pageCharacterSelect.OnConfirmCharacters.RemoveListener(WhenCharactersConfirmed);
            pageCharacterSelect.OnCancel.RemoveListener(WhenCharactersCanceled);
            _ = screenManager.TryBackPage();
            
            if (gamemodeConfigurationEnabled == false)
            {
                SetupGamemodePickScreen();
                return;
            }
            SetupConfigurationScreen();
        }
        
        private async void WhenCharactersConfirmed()
        {
            Debug.Log("Characters Confirmed");
            pageCharacterSelect.OnConfirmCharacters.RemoveListener(WhenCharactersConfirmed);
            pageCharacterSelect.OnCancel.RemoveListener(WhenCharactersCanceled);

            var charactersPicked = pageCharacterSelect.GetCharactersPicked();
            pageCharacterSelect.Teardown();

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
                _ = screenManager.TryBackPage(); // TODO: Hook the events back up
                return;
            }
            
            SetupMapPick();
            _ = screenManager.TryForwardPage(pageContentPicking);
        }

        // Map Picking
        private void SetupMapPick()
        {
            var gameManager = HnSFManagersContainer.instance;
            pageContentPicking.onContentPicked.AddListener(OnMapPicked);
            pageContentPicking.onCancel.AddListener(OnMapPickCanceled);
            pageContentPicking.Initialize<IMapDefinition>();
        }

        private void TeardownMapPick()
        {
            pageContentPicking.onContentPicked.RemoveListener(OnMapPicked);
            pageContentPicking.onContentPicked.RemoveListener(OnMapPickCanceled);
        }
        
        private void OnMapPickCanceled(GenericContentPickerInstance arg0)
        {
            TeardownMapPick();
            _ = screenManager.TryBackPage();
            SetupCharacterSelect();
        }

        private async void OnMapPicked(GenericContentPickerInstance arg0)
        {
            TeardownMapPick();
            
            if (mapAssetHandle.IsValid())
            {
                HnSFManagersContainer.instance.contentManager.ReleaseAssetFromMod(mapAssetHandle);
            }
            mapAssetHandle = pageContentPicking.ConfirmWantedContentAndRemoveFromList();
            pageContentPicking.Uninitialize();
            
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