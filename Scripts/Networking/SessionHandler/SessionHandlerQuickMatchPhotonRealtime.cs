using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    public partial class SessionHandlerQuickMatchPhotonRealtime : SessionHandlerBase, IConnectionCallbacks,
        IMatchmakingCallbacks, IOnEventCallback, IInRoomCallbacks
    {
        private TypedLobby sqlLobby = new TypedLobby("customSqlLobby", LobbyType.Sql);
        public const string LOBBY_TYPE_PROP_KEY = "C4";
        public const string MOD_GUID_PROP_KEY = "C0";
        public const string GAME_MODE_PROP_KEY = "C1";
        public const string PLAYER_COUNT_PROP_KEY = "C2";
        public const string MAP_PROP_KEY = "C3";

        public const string CLIENT_READY_STATE_PROP_KEY = "ready";
        public const string CLIENT_LOCAL_PLAYER_COUNT_PROP_KEY = "clp";

        public UnityEvent<Room> OnRoomCreated = new();
        public UnityEvent<string> OnMatchEnded = new();

        public AppSettings quantumAppSettings;
        public RealtimeClient quantumClient = null;
        private string matchRegion = "usw";

        public int minClientCount = 2;
        public int maxClientCount = 4;
        public int minPlayerCount = 2;
        public int maxPlayerCount = 4;

        // Local Info
        private string clientSecret;
        public string localUsername;
        public string localModGuid;
        public LoadedAssetHandleWrapper selectedGamemodeDefinition;
        public List<PlayerMatchContentBundle> localPlayerContentBundles = new List<PlayerMatchContentBundle>();

        public RuntimeConfig defaultRuntimeConfig;

        public Dictionary<byte, Action<SessionHandlerQuickMatchPhotonRealtime, EventData>> receivedEventHandlers =
            new Dictionary<byte, Action<SessionHandlerQuickMatchPhotonRealtime, EventData>>();

        // Match Info
        public Dictionary<Player, int> room_ClientLocalPlayerCounts = new Dictionary<Player, int>();
        public int currentRealPlayerCount;
        public LoadedAssetHandleWrapper selectedMapDefinition;

        // Asset Management
        //public List<LoadedAssetHandleWrapper> assetHandles = new List<LoadedAssetHandleWrapper>();
        public MatchHandlerBase matchHandlerInstance;

        // ...
        public float timestampLastPlayerJoinedRoom;

        // ...
        public SessionHandlerMatch matchSessionHandlerPrefab;
        public SessionHandlerMatch matchSessionHandler;
        
        public override bool Initialize()
        {
            defaultRuntimeConfig.SystemsConfig = QuantumDefaultConfigs.Global.SystemsConfig;
            defaultRuntimeConfig.SimulationConfig = QuantumDefaultConfigs.Global.SimulationConfig;
            
            clientSecret = $"a{Random.Range(0, 10000)}_{Random.Range(0, 10000)}_{Time.realtimeSinceStartup}";
            localModGuid = HnSFManagersContainer.instance.modManager.GenerateModGuid().ToString();
            PhotonServerSettings.Global.AppSettings.CopyTo(quantumAppSettings);
            receivedEventHandlers.Add(110, Messages.Received_StartGame);
            return base.Initialize();
        }

        public override void Teardown()
        {
            if (TornDown)
                return;
            
            if (HnSFManagersContainer.instance == null) return;
            
            var contentManager = HnSFManagersContainer.instance.contentManager;
            
            if (matchSessionHandler)
            {
                HnSFManagersContainer.instance.sessionHandlerManager.DestroySessionHandler("QuickMatch-QuantumGame");
                matchSessionHandler = null;
            }
            
            if(selectedGamemodeDefinition.IsValid()) contentManager.ReleaseAssetFromMod(selectedGamemodeDefinition);
            if(selectedMapDefinition.IsValid()) contentManager.ReleaseAssetFromMod(selectedMapDefinition);
            //if(selectedSongDefinition.IsValid()) contentManager.ReleaseAssetFromMod(selectedSongDefinition);

            selectedGamemodeDefinition = default;
            selectedMapDefinition = default;
            //selectedSongDefinition = default;
            
            if (matchHandlerInstance != null)
            {
                matchHandlerInstance.Teardown();
            }

            if (matchSessionHandler != null)
            {
                matchSessionHandler.Teardown();
            }
            
            quantumClient?.Disconnect();
            quantumClient = null;
            
            base.Teardown();
        }
        
        private void OnDestroy()
        {
            Teardown();
        }
        
        private void WhenQuitMatch(int arg0)
        {
            OnMatchEnded.Invoke(null);
            GameObject.Destroy(gameObject);
        }
        
        public void Update()
        {
            quantumClient?.Service();
            
            if (quantumClient == null) return;
            if (matchSessionHandler != null && matchSessionHandler.inProgress) return;
            AttemptStartMatch();
        }

        private bool attemptedMatchStart = false;
        private void AttemptStartMatch()
        {
            if (attemptedMatchStart) return;
            if (currentRealPlayerCount < minPlayerCount || quantumClient == null || !quantumClient.InRoom 
                || !quantumClient.LocalPlayer.IsMasterClient || quantumClient.CurrentRoom.PlayerCount < minClientCount) return;
            if (Time.realtimeSinceStartup - timestampLastPlayerJoinedRoom < 10) return;
            attemptedMatchStart = true;

            Messages.Send_StartGame(this);
            timestampLastPlayerJoinedRoom = Time.realtimeSinceStartup;
        }

        public async UniTask TransitionToQuantumGameSession()
        {
            var sessionManager = HnSFManagersContainer.instance.sessionHandlerManager;

            matchSessionHandler = sessionManager.CreateSessionHandler("QuickMatch-QuantumGame", matchSessionHandlerPrefab);
            if (matchSessionHandler == null) return;
            matchSessionHandler.OnQuitMatch.AddListener(WhenQuitMatch);
            
            // GAMEMODE SYSTEMS
            if (QuantumUnityDB.TryGetGlobalAsset(matchHandlerInstance.gamemodeSystemConfig,
                    out var gamemodeSystemConfigOverrider))
            {
                var builtSystemConfig = gamemodeSystemConfigOverrider.BuildSystemsConfig();
                builtSystemConfig.name = $"SYSTEMCONFIG_{matchHandlerInstance.name}_{gamemodeSystemConfigOverrider.name}";
                var guid = QuantumUnityDB.CreateRuntimeDeterministicGuid(builtSystemConfig);
                builtSystemConfig.Guid = guid;
                QuantumUnityDB.Global.AddAsset(builtSystemConfig);
                defaultRuntimeConfig.SystemsConfig = builtSystemConfig;
            }
            else
            {
                Debug.LogError("No system config provided in match handler.");
            }
            
            // GAMEMODE CONFIG
            // TODO
            
            // MAP
            defaultRuntimeConfig.Map.Id = selectedMapDefinition.GetAsset<IMapDefinition>().GetMapAsset().Guid.Value;
            await selectedMapDefinition.GetAsset<IMapDefinition>().LoadMap(LoadSceneMode.Single);
            
            int gameFlags = 0;
            
            var sessionRunnerArgs = new SessionRunner.Arguments
            {
                RunnerFactory = QuantumRunnerUnityFactory.DefaultFactory,
                GameParameters = QuantumRunnerUnityFactory.CreateGameParameters,
                ClientId = clientSecret, // TODO: Actual client secret.
                RuntimeConfig = defaultRuntimeConfig,
                SessionConfig = QuantumDeterministicSessionConfigAsset.DefaultConfig,
                GameMode = DeterministicGameMode.Multiplayer,
                PlayerCount = quantumClient.CurrentRoom.PlayerCount,
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
        
        public async UniTask<bool> TryPrepareForMatchmaking()
        {
            var gamemodeSetupResult = await InitializeMatchHandler();
            if (gamemodeSetupResult == false) return false;
            AttemptMasterServerConnection();
            return true;
        }

        private async Task<bool> InitializeMatchHandler()
        {
            var gamemodeDefinition = selectedGamemodeDefinition.GetAsset<BaseGamemodeDefinition>();
            if (gamemodeDefinition == null) return false;
            if ((await gamemodeDefinition.LoadAssets()) == false)
            {
                gamemodeDefinition.UnloadAssets();
                return false;
            }

            matchHandlerInstance =
                GameObject.Instantiate(gamemodeDefinition.GetMatchHandler().GetComponent<MatchHandlerBase>(), transform,
                    false);
            minPlayerCount = gamemodeDefinition.MinimumPlayers;
            maxPlayerCount = gamemodeDefinition.MaximumPlayers;
            return true;
        }

        private void AttemptMasterServerConnection()
        {
            quantumClient = new RealtimeClient(PhotonServerSettings.Global.AppSettings.Protocol);
            quantumAppSettings.FixedRegion = matchRegion;

            quantumClient.UserId = localUsername;
            if (quantumClient.ConnectUsingSettings((quantumAppSettings)))
            {
                quantumClient.AddCallbackTarget(this);
                Debug.Log("Setup quantum game.");
            }
            else
            {
                Debug.LogError($"Failed to connect with app settings: '{quantumAppSettings.ToStringFull()}'");
            }
        }

        private void AttemptJoinValidRoom()
        {
            string sqlLobbyFilter =
                $"{MOD_GUID_PROP_KEY} = '{localModGuid}' " +
                $"AND {PLAYER_COUNT_PROP_KEY} <= {maxPlayerCount - localPlayerContentBundles.Count} " +
                $"AND {GAME_MODE_PROP_KEY} = '{selectedGamemodeDefinition.assetReference.ToString()}'";
            var opJoinRandomRoomParams = new JoinRandomRoomArgs();
            opJoinRandomRoomParams.MatchingType = MatchmakingMode.FillRoom;
            opJoinRandomRoomParams.Lobby = sqlLobby;
            opJoinRandomRoomParams.SqlLobbyFilter = sqlLobbyFilter;
            quantumClient.OpJoinRandomRoom(opJoinRandomRoomParams);
        }

        private async UniTask<bool> AttemptCreateRoom()
        {
            var mapPicker = new GenericContentListingUtility<IMapDefinition>();
            mapPicker.Initialize();
            if (mapPicker.currentAssetList.Count == 0)
            {
                Debug.LogError("No maps to select from!");
                return false;
            }

            var contentMananger = HnSFManagersContainer.instance.contentManager;
            var loadResult = await contentMananger.LoadAssetFromModAsync(mapPicker.currentAssetList[0]);
            if (loadResult.result == false)
            {
                Debug.Log("Could not load map.");
                return false;
            }
            selectedMapDefinition = loadResult.handle;

            await selectedMapDefinition.GetAsset<IMapDefinition>().LoadAssets();
            
            currentRealPlayerCount = localPlayerContentBundles.Count;
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.IsVisible = true;
            roomOptions.MaxPlayers = maxClientCount;
            roomOptions.Plugins = new string[] { "QuantumPlugin" };
            roomOptions.PlayerTtl = PhotonServerSettings.Global.PlayerTtlInSeconds * 1000;
            roomOptions.EmptyRoomTtl = PhotonServerSettings.Global.EmptyRoomTtlInSeconds * 1000;

            roomOptions.CustomRoomPropertiesForLobby = new object[] { MOD_GUID_PROP_KEY, GAME_MODE_PROP_KEY, PLAYER_COUNT_PROP_KEY };
            roomOptions.CustomRoomProperties = new PhotonHashtable()
            {
                { MOD_GUID_PROP_KEY, localModGuid },
                { GAME_MODE_PROP_KEY, selectedGamemodeDefinition.assetReference.ToString() },
                { PLAYER_COUNT_PROP_KEY, currentRealPlayerCount },
                { MAP_PROP_KEY, selectedMapDefinition.assetReference.ToString() },
                { "HIDE-ROOM", false },
                { "STARTED", false }
            };
            var enterRoomParams = new EnterRoomArgs();
            enterRoomParams.RoomOptions = roomOptions;
            enterRoomParams.Lobby = sqlLobby;
            quantumClient.OpCreateRoom(enterRoomParams);
            return true;
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

            AttemptJoinValidRoom();
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

        public void OnFriendListUpdate(List<FriendInfo> friendList)
        {
        }

        public void OnCreatedRoom()
        {
            Debug.Log($"Created room. Name: \"{quantumClient.CurrentRoom.Name}\"");
            OnRoomCreated.Invoke(quantumClient.CurrentRoom);
            Debug.Log(quantumClient.CurrentRoom.CustomProperties.ToStringFull());
        }

        public void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"Create room failed [{returnCode}]: {message}");
            quantumClient?.Disconnect();
        }

        public void OnJoinedRoom()
        {
            Debug.Log($"Entered room '{quantumClient.CurrentRoom.Name}' as actor '{quantumClient.LocalPlayer.ActorNumber}'");

            // Send init parameters.
            quantumClient.LocalPlayer.SetCustomProperties(new PhotonHashtable()
            {
                { CLIENT_LOCAL_PLAYER_COUNT_PROP_KEY, localPlayerContentBundles.Count },
                { CLIENT_READY_STATE_PROP_KEY, false }
            });
        }

        public void OnJoinRoomFailed(short returnCode, string message)
        {
            quantumClient?.Disconnect();
        }

        public void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log($"Quick Match failed to join random room [{returnCode}]: {message}. Attempting to create room instead.");
            _ = AttemptCreateRoom();
        }

        public void OnLeftRoom()
        {
            quantumClient?.Disconnect();
        }

        #endregion

        #region IOnEventCallback

        public void OnEvent(EventData photonEvent)
        {
            if (!receivedEventHandlers.ContainsKey((byte)photonEvent.Code))
            {
                return;
            }

            receivedEventHandlers[(byte)photonEvent.Code].Invoke(this, photonEvent);
        }

        #endregion

        #region IInRoomCallbacks

        public void OnPlayerEnteredRoom(Player newPlayer)
        {
            timestampLastPlayerJoinedRoom = Time.realtimeSinceStartup;
        }

        public void OnPlayerLeftRoom(Player otherPlayer)
        {
            // TODO : Remove player from count.
        }

        public void OnRoomPropertiesUpdate(PhotonHashtable propertiesThatChanged)
        {
            Debug.Log($"Room property updated {propertiesThatChanged.ToStringFull()}");
        }

        public void OnPlayerPropertiesUpdate(Player targetPlayer, PhotonHashtable changedProps)
        {
            Debug.Log($"{targetPlayer.ActorNumber} changed their properties.\n{changedProps.ToStringFull()}");
            
            if (!targetPlayer.IsMasterClient && quantumClient.LocalPlayer.IsMasterClient
                && changedProps.ContainsKey(CLIENT_LOCAL_PLAYER_COUNT_PROP_KEY))
            {
                if (room_ClientLocalPlayerCounts.ContainsKey(targetPlayer))
                {
                    return;
                }

                var targetClientPlayerCount = (int)changedProps[CLIENT_LOCAL_PLAYER_COUNT_PROP_KEY];
                if (targetClientPlayerCount > (maxPlayerCount - currentRealPlayerCount))
                {
                    Debug.LogError($"{targetPlayer} can't fit in match.");
                    // TODO: Disconnect them.
                    return;
                }

                room_ClientLocalPlayerCounts.Add(targetPlayer, targetClientPlayerCount);
                currentRealPlayerCount += targetClientPlayerCount;
                quantumClient.CurrentRoom.SetCustomProperties(new PhotonHashtable()
                {
                    { PLAYER_COUNT_PROP_KEY, currentRealPlayerCount }
                });
                Debug.Log($"{targetPlayer} added players to room. Current real player count is {currentRealPlayerCount}");
            }

            if (changedProps.ContainsKey(CLIENT_READY_STATE_PROP_KEY))
            {
                
            }
        }

        public void OnMasterClientSwitched(Player newMasterClient)
        {
            quantumClient?.Disconnect();
        }

        #endregion
    }
}