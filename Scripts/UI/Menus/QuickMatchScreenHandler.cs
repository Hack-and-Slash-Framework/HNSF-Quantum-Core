using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HnSF.sessionhandling.handlers;
using UnityEngine;
using UnityEngine.Events;

namespace HnSF.ui.menus
{
    public class QuickMatchScreenHandler : MenuHandlerBase
    {
        [System.Serializable]
        public class QuickMatchLocalPlayerInfo
        {
            public int PlayerId
            {
                get => playerId;
                set
                {
                    if (playerId == value) return;
                    playerId = value;
                    OnInfoUpdated.Invoke(this);
                }
            }

            public string PlayerName
            {
                get => playerName;
                set
                {
                    if(playerName == value) return;
                    playerName = value;
                    OnInfoUpdated.Invoke(this);
                }
            }

            public bool Ready
            {
                get => ready;
            }

            public UnityEvent<QuickMatchLocalPlayerInfo> OnInfoUpdated = new UnityEvent<QuickMatchLocalPlayerInfo>();
            
            protected int playerId;
            protected string playerName;
            protected bool ready;

            public LoadedAssetHandleWrapper assetHandleCharacter;

            public void SetCharacterAssetHandle(LoadedAssetHandleWrapper characterAssetHandle)
            {
                assetHandleCharacter = characterAssetHandle;
            }
            
            public void SetReady(bool value, bool callUpdateEvent = true)
            {
                if (ready == value) return;
                ready = value;
                if(callUpdateEvent) OnInfoUpdated.Invoke(this);
            }
        }
        
        public QuickMatchScreenInstance quickMatchScreenInstancePrefab;
        public Camera instanceCameraPrefab;
        
        public Dictionary<int, QuickMatchScreenInstance> playerIdToScreenInstance =
            new Dictionary<int, QuickMatchScreenInstance>();
        
        public Dictionary<int, QuickMatchLocalPlayerInfo> localPlayersInfo = new Dictionary<int, QuickMatchLocalPlayerInfo>();

        public LoadedAssetHandleWrapper selectedGamemodeDefinition;

        public SessionHandlerQuickMatchPhotonRealtime sessionHandlerPrefab;
        
        public bool Open()
        {
            playerIdToScreenInstance.Clear();
            localPlayersInfo.Clear();
            
            quickMatchScreenInstancePrefab.transform.parent.gameObject.SetActive(false);
            gameObject.SetActive(true);

            var inputPlayerManager = HnSFManagersContainer.instance.inputManager;
            var splitscreenManager = HnSFManagersContainer.instance.splitScreenManager;
            
            foreach (var inputPlayer in inputPlayerManager.GetPlayers())
            {
                localPlayersInfo.Add(inputPlayer.Id, new QuickMatchLocalPlayerInfo());
                localPlayersInfo[inputPlayer.Id].PlayerId = inputPlayer.Id;
                var screenInstance = Instantiate(quickMatchScreenInstancePrefab, transform, false);
                playerIdToScreenInstance.Add(inputPlayer.Id, screenInstance);
                screenInstance.inputPlayer = inputPlayer;
                screenInstance.playerInfo = localPlayersInfo[inputPlayer.Id];
                screenInstance.instanceCamera = GameObject.Instantiate(instanceCameraPrefab, transform, false);
                localPlayersInfo[inputPlayer.Id].OnInfoUpdated.AddListener(WhenLocalPlayerInfoUpdated);
                screenInstance.instanceHandler = this;
                screenInstance.Open();
                splitscreenManager.RegisterCamera(screenInstance.instanceCamera);
            }
            
            splitscreenManager.Activate();

            return true;
        }

        public void Close()
        {
            foreach (var playerScreenInstance in playerIdToScreenInstance.Values)
            {
                playerScreenInstance.Close();
                GameObject.Destroy(playerScreenInstance.instanceCamera);
                GameObject.Destroy(playerScreenInstance.gameObject);
            }
            
            playerIdToScreenInstance.Clear();
            gameObject.SetActive(false);
        }
        
        private void WhenLocalPlayerInfoUpdated(QuickMatchLocalPlayerInfo arg0)
        {
            Debug.Log($"{arg0.PlayerId} updated info.");

            _ = CheckForAllReadyAndStart();
        }

        private async UniTaskVoid CheckForAllReadyAndStart()
        {
            bool allReady = true;
            foreach (var playerIdToInfo in localPlayersInfo)
            {
                var playerInfo = playerIdToInfo.Value;
                if (playerInfo.Ready == false)
                {
                    allReady = false;
                    break;
                }
            }

            if (allReady == false) return;
            
            Debug.Log("READY ALL");
            var managers = HnSFManagersContainer.instance;

            var sessionHandler = managers.sessionHandlerManager.CreateSessionHandler("quickmatch-lobby", sessionHandlerPrefab);
            if (sessionHandler == null) return;

            sessionHandler.selectedGamemodeDefinition = selectedGamemodeDefinition;
            foreach(var lpi in localPlayersInfo)
            {
                var contentBundle = new PlayerMatchContentBundle();
                await contentBundle.Create(new List<LoadedAssetHandleWrapper>() { lpi.Value.assetHandleCharacter }, 0);

                sessionHandler.localPlayerContentBundles.Add(contentBundle);
            }

            _ = sessionHandler.TryPrepareForMatchmaking();
        }
    }
}