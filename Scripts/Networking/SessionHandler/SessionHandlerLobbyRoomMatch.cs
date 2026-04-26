using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Photon.Client;
using Photon.Deterministic;
using Photon.Realtime;
using Quantum;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace HnSF.sessionhandling.handlers
{
    // TODO : Handle starting errors (probably via events).
    public class SessionHandlerLobbyRoomMatch : SessionHandlerBase, IConnectionCallbacks,
        IMatchmakingCallbacks, IOnEventCallback, IInRoomCallbacks
    {
        public AppSettings quantumAppSettings;
        public RealtimeClient quantumClient = null;
        public RuntimeConfig defaultRuntimeConfig;
        public string matchRegion = "usw";

        private string clientSecret;
        public string localUsername;

        public int expectedClientCount;
        public int expectedPlayerCount;

        public string matchCode;

        public UnityEvent<Room> OnRoomCreated = new();
        public UnityEvent<Room> OnRoomJoined = new();
        public UnityEvent<string> OnMatchEnded = new();

        // Asset Management
        //public List<LoadedAssetHandleWrapper> assetHandles = new List<LoadedAssetHandleWrapper>();
        public LoadedAssetHandleWrapper selectedGamemodeDefinition;
        public LoadedAssetHandleWrapper selectedMapDefinition;
        public LoadedAssetHandleWrapper selectedSongDefinition;
        public string gamemodeConfiguration;
        public MatchHandlerBase matchHandlerInstance;
        public List<PlayerMatchContentBundle> localPlayerContentBundles = new List<PlayerMatchContentBundle>();
        public SystemsConfig builtSystemConfig;
        public GamemodeSettingsBase settingsAssetInstance;
        
        // ...
        public SessionHandlerMatch matchSessionHandlerPrefab;
        public SessionHandlerMatch matchSessionHandler;

        public double roomWaitStartTime;
        
        public override bool Initialize()
        {
            defaultRuntimeConfig.SystemsConfig = QuantumDefaultConfigs.Global.SystemsConfig;
            defaultRuntimeConfig.SimulationConfig = QuantumDefaultConfigs.Global.SimulationConfig;
            clientSecret = $"a{Random.Range(0, 10000)}_{Random.Range(0, 10000)}_{Time.realtimeSinceStartup}";
            PhotonServerSettings.Global.AppSettings.CopyTo(quantumAppSettings);
            return base.Initialize();
        }

        public override void Teardown()
        {
            if (HnSFManagersContainer.instance == null) return;
            var contentManager = HnSFManagersContainer.instance.contentManager;

            if (builtSystemConfig)
            {
                QuantumUnityDB.Global.RemoveSource(builtSystemConfig.Guid);
                Destroy(builtSystemConfig);
            }

            if (settingsAssetInstance)
            {
                QuantumUnityDB.Global.RemoveSource(settingsAssetInstance.Guid);
                Destroy(settingsAssetInstance);
            }
            
            if(selectedGamemodeDefinition.IsValid()) contentManager.ReleaseAssetFromMod(selectedGamemodeDefinition);
            if(selectedMapDefinition.IsValid()) contentManager.ReleaseAssetFromMod(selectedMapDefinition);
            if(selectedSongDefinition.IsValid()) contentManager.ReleaseAssetFromMod(selectedSongDefinition);

            selectedGamemodeDefinition = default;
            selectedMapDefinition = default;
            selectedSongDefinition = default;
            
            if (matchHandlerInstance != null)
            {
                matchHandlerInstance.Teardown();
            }

            if (matchSessionHandler != null)
            {
                matchSessionHandler.Teardown();
            }
            
            quantumClient.Disconnect();
            quantumClient = null;
        }

        public void ForceQuit()
        {
            if (matchSessionHandler)
            {
                HnSFManagersContainer.instance.sessionHandlerManager.DestroySessionHandler("ReplaceThis");
            }
        }

        protected virtual void OnDestroy()
        {
            Teardown();
        }
        
        protected virtual void WhenQuitMatch(int arg0)
        {
            ForceQuit();
            OnMatchEnded.Invoke(null);
        }
        
        public virtual async UniTask<bool> PrepareForMatchAndJoin(QuantumMatchContentBundle matchContentBundle)
        {
            localPlayerContentBundles = matchContentBundle.localPlayerBundles;
            Debug.Log("Preparing for match.");
            var contentManager = HnSFManagersContainer.instance.contentManager;
            
            var gamemodeLoadResult = await contentManager.LoadAssetFromModAsync(matchContentBundle.gamemodeReference);
            this.selectedGamemodeDefinition = gamemodeLoadResult.handle;
            
            var mapLoadResult = await contentManager.LoadAssetFromModAsync(matchContentBundle.mapReference);
            this.selectedMapDefinition = mapLoadResult.handle;

            this.expectedClientCount = matchContentBundle.clientCount;
            this.expectedPlayerCount = matchContentBundle.playerCount;

            this.gamemodeConfiguration = matchContentBundle.gamemodeSettings;
            
            var gamemodeSetupResult = await InitializeMatchHandler();
            if (gamemodeSetupResult == false) return false;
            AttemptMasterServerConnection();
            return true;
        }

        protected virtual void Update()
        {
            if (matchSessionHandler != null) return;
            quantumClient?.Service();
            
            AttemptStartMatch();
        }

        private bool attemptedMatchStart = false;
        protected virtual void AttemptStartMatch()
        {
            if (attemptedMatchStart) return;
            if (quantumClient == null || !quantumClient.InRoom || !quantumClient.LocalPlayer.IsMasterClient) return;
            if (ShouldStartQuantumGame() == false) return;
            attemptedMatchStart = true;
            
            quantumClient.OpRaiseEvent((byte)110, null, 
                new RaiseEventArgs { Receivers = ReceiverGroup.All },
                SendOptions.SendReliable);
        }

        protected virtual bool ShouldStartQuantumGame()
        {
            var allClientsInRoom = quantumClient.CurrentRoom.PlayerCount == expectedClientCount;
            var joinWaitTimeIsUp = roomWaitStartTime > 0 && Time.realtimeSinceStartupAsDouble > roomWaitStartTime;

            return allClientsInRoom || joinWaitTimeIsUp;
        }

        protected virtual async UniTask TransitionToQuantumGameSession()
        {
            var sessionManager = HnSFManagersContainer.instance.sessionHandlerManager;

            matchSessionHandler = sessionManager.CreateSessionHandler("ReplaceThis", matchSessionHandlerPrefab);
            if (matchSessionHandler == null)
            {
                Debug.LogError("Could not create match session handler.");
                return;
            }
            matchSessionHandler.OnQuitMatch.AddListener(WhenQuitMatch);
            
            // GAMEMODE SYSTEMS
            BuildGamemodeSystemsConfig();
            
            // GAMEMODE CONFIG
            BuildGamemodeSettingsAsset();
            
            // MATCH HANDLER SETUP
            bool gamemodeSetupResult = await matchHandlerInstance.Setup(defaultRuntimeConfig);
            
            // MAP
            await LoadMap();
            await UniTask.NextFrame();
            
            await PreSessionCreation();
            
            int gameFlags = 0;
            var sessionRunnerArgs = new SessionRunner.Arguments
            {
                RunnerFactory = QuantumRunnerUnityFactory.DefaultFactory,
                GameParameters = QuantumRunnerUnityFactory.CreateGameParameters,
                ClientId = clientSecret, // TODO: Actual client secret.
                RuntimeConfig = defaultRuntimeConfig,
                SessionConfig = QuantumDeterministicSessionConfigAsset.DefaultConfig,
                GameMode = DeterministicGameMode.Multiplayer,
                PlayerCount = expectedPlayerCount,
                StartGameTimeoutInSeconds = 10.0f,
                GameFlags = gameFlags,
                Communicator = new QuantumNetworkCommunicator(quantumClient, ShutdownConnectionOptions.Disconnect),
                RecordingFlags = RecordingFlags.None,
#if UNITY_EDITOR
                DeltaTimeType = SimulationUpdateTime.EngineDeltaTime
#endif
            };

            matchHandlerInstance.SetLocalPlayerInfo(localPlayerContentBundles);
            
            matchSessionHandler.runner = (QuantumRunner)await SessionRunner.StartAsync(sessionRunnerArgs);
            matchSessionHandler.matchHandlerInstance = matchHandlerInstance;
            matchSessionHandler.selectedMapDefinition = selectedMapDefinition;
            matchSessionHandler.InitMatch();

            quantumClient.RemoveCallbackTarget(this);
        }
        
        protected virtual void BuildGamemodeSystemsConfig()
        {
            if (QuantumUnityDB.TryGetGlobalAsset(matchHandlerInstance.gamemodeSystemConfig,
                    out var gamemodeSystemConfigOverrider))
            {
                builtSystemConfig = gamemodeSystemConfigOverrider.BuildSystemsConfig();
                builtSystemConfig.name = $"SYSTEMCONFIG_{matchHandlerInstance.name}_{gamemodeSystemConfigOverrider.name}";
                builtSystemConfig.Guid = QuantumUnityDB.CreateRuntimeDeterministicGuid(builtSystemConfig);
                QuantumUnityDB.Global.AddAsset(builtSystemConfig);
                defaultRuntimeConfig.SystemsConfig = builtSystemConfig;
            }
            else
            {
                Debug.LogError("No system config provided in match handler.");
            }
        }
        
        protected virtual void BuildGamemodeSettingsAsset()
        {
            var gamemodeDefinition = selectedGamemodeDefinition.GetAsset<BaseGamemodeDefinition>();
            if (!string.IsNullOrEmpty(gamemodeConfiguration))
            {
                try
                {
                    settingsAssetInstance =
                        JsonUtilityExtensions.FromJsonWithTypeAnnotation(gamemodeConfiguration) as GamemodeSettingsBase;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Exception thrown while trying to load gamemode settings asset: {e}. Using default settings.");
                    settingsAssetInstance = matchHandlerInstance.defaultSettings.GetInstance();
                }
            }
            
            if(settingsAssetInstance == null)
            {
                settingsAssetInstance = matchHandlerInstance.defaultSettings.GetInstance();
            }
            
            // Register instance to quantum
            settingsAssetInstance.name = $"GamemodeSettingsAsset_RUNTIME_{gamemodeDefinition.Name}";
            settingsAssetInstance.Guid = QuantumUnityDB.CreateRuntimeDeterministicGuid(settingsAssetInstance);
            QuantumUnityDB.Global.AddAsset(settingsAssetInstance);
            
            defaultRuntimeConfig.gamemodeConfigAsset = settingsAssetInstance;
            
            // Set settings
            settingsAssetInstance.teamRules = gamemodeDefinition.GetTeamRules();
            settingsAssetInstance.teamConfigs = gamemodeDefinition.GetDefaultTeamConfig();
            settingsAssetInstance.Initialize();
        }
        
        protected virtual async UniTask LoadMap()
        {
            await selectedMapDefinition.GetAsset<IMapDefinition>().LoadAssets();
            defaultRuntimeConfig.Map.Id = selectedMapDefinition.GetAsset<IMapDefinition>().GetMapAsset().Guid.Value;
            await selectedMapDefinition.GetAsset<IMapDefinition>().LoadMap(LoadSceneMode.Single);
        }
        
        protected virtual UniTask PreSessionCreation()
        {
            defaultRuntimeConfig.Seed = Random.Range(int.MinValue, int.MaxValue);
            return UniTask.CompletedTask;
        }

        protected virtual async UniTask<bool> InitializeMatchHandler()
        {
            var gamemodeDefinition = selectedGamemodeDefinition.GetAsset<BaseGamemodeDefinition>();
            if (gamemodeDefinition == null) return false;
            if ((await gamemodeDefinition.LoadAssets()) == false)
            {
                gamemodeDefinition.UnloadAssets();
                return false;
            }

            matchHandlerInstance = GameObject.Instantiate(gamemodeDefinition.GetMatchHandler().GetComponent<MatchHandlerBase>(), transform,
                    false);
            return true;
        }

        protected virtual void AttemptMasterServerConnection()
        {
            quantumClient = new RealtimeClient(PhotonServerSettings.Global.AppSettings.Protocol);
            quantumAppSettings.FixedRegion = matchRegion;

            quantumClient.UserId = localUsername;
            
            if (quantumClient.ConnectUsingSettings((quantumAppSettings)))
            {
                Debug.Log("Attempting master server connection.");
                quantumClient.AddCallbackTarget(this);
            }
            else
            {
                Debug.LogError($"Failed to connect with app settings: '{quantumAppSettings.ToStringFull()}'");
                quantumClient.RemoveCallbackTarget(this);
            }
        }
        
        protected virtual void AttemptJoinRoom()
        {
            EnterRoomArgs opJoinRoomParams = new EnterRoomArgs();
            opJoinRoomParams.RoomName = matchCode;
            quantumClient.OpJoinRoom(opJoinRoomParams);
        }
        
        protected virtual void AttemptCreateRoom()
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.IsVisible = true;
            roomOptions.MaxPlayers = expectedClientCount;
            roomOptions.Plugins = new string[] { "QuantumPlugin" };
            roomOptions.PlayerTtl = PhotonServerSettings.Global.PlayerTtlInSeconds * 1000;
            roomOptions.EmptyRoomTtl = PhotonServerSettings.Global.EmptyRoomTtlInSeconds * 1000;
            roomOptions.IsVisible = false;
            
            roomOptions.CustomRoomProperties = new PhotonHashtable()
            {
                { "HIDE-ROOM", true },
                { "STARTED", false }
            };
            var enterRoomParams = new EnterRoomArgs();
            enterRoomParams.RoomOptions = roomOptions;
            quantumClient.OpCreateRoom(enterRoomParams);
        }

        #region IConnectionCallbacks

        public void OnConnected()
        {
        }

        private EnterRoomArgs _enterRoomParams;

        public void OnConnectedToMaster()
        {
            if (string.IsNullOrEmpty(quantumClient.CurrentRegion) == false)
            {
                Debug.Log($"Connected to master server in region '{quantumClient.CurrentRegion}'");
            }
            else
            {
                Debug.Log($"Connected to master server '{quantumClient.MasterServerAddress}'");
            }

            Debug.Log($"UserId: {quantumClient.UserId}");

            if (string.IsNullOrEmpty(matchCode))
            {
                AttemptCreateRoom();
            }
            else
            {
                AttemptJoinRoom();
            }
        }

        public void OnDisconnected(DisconnectCause cause)
        {
        }

        public void OnRegionListReceived(RegionHandler regionHandler)
        {
        }

        public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
        {
        }

        public void OnCustomAuthenticationFailed(string debugMessage)
        {
        }

        #endregion

        #region IMatchmakingCallbacks

        public void OnFriendListUpdate(List<FriendInfo> friendList) { }

        public void OnCreatedRoom()
        {
            roomWaitStartTime = Time.realtimeSinceStartupAsDouble + 15;
            Debug.Log($"Created room. Name: \"{quantumClient.CurrentRoom.Name}\"");
            OnRoomCreated.Invoke(quantumClient.CurrentRoom);
        }

        public void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"Create room failed [{returnCode}]: {message}");
            quantumClient?.Disconnect();
        }

        public void OnJoinedRoom()
        {
            Debug.Log($"Entered room '{quantumClient.CurrentRoom.Name}' as actor '{quantumClient.LocalPlayer.ActorNumber}'");
            OnRoomJoined.Invoke(quantumClient.CurrentRoom);
        }

        public void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.Log($"Failed to join room [{returnCode}]: {message}. Disconnecting.");
            quantumClient?.Disconnect();
        }

        public void OnJoinRandomFailed(short returnCode, string message) { }

        public void OnLeftRoom()
        {
            quantumClient?.Disconnect();
        }

        #endregion

        #region IOnEventCallback

        public void OnEvent(EventData photonEvent)
        {
            var code = (byte)photonEvent.Code;
            switch (code)
            {
                case 110:
                    if (quantumClient.LocalPlayer.IsMasterClient)
                    {
                        // Save the started state in room properties for late joiners
                        var ht = new PhotonHashtable { { "STARTED", true } };
                        quantumClient.CurrentRoom.SetCustomProperties(ht);

                        if (quantumClient.CurrentRoom.CustomProperties.TryGetValue("HIDE-ROOM", out var hideRoom) && (bool)hideRoom)
                        {
                            quantumClient.CurrentRoom.IsVisible = false;
                        }
                    }

                    _ = TransitionToQuantumGameSession();
                    break;
            }
        }

        #endregion

        #region IInRoomCallbacks
        public void OnPlayerEnteredRoom(Player newPlayer)
        {
            //timestampLastPlayerJoinedRoom = Time.realtimeSinceStartup;
        }

        public void OnPlayerLeftRoom(Player otherPlayer) { }

        public void OnRoomPropertiesUpdate(PhotonHashtable propertiesThatChanged)
        {
            //Debug.Log($"Room property updated {propertiesThatChanged.ToStringFull()}");
        }

        public void OnPlayerPropertiesUpdate(Player targetPlayer, PhotonHashtable changedProps) { }

        public void OnMasterClientSwitched(Player newMasterClient) { }
        #endregion
    }
}