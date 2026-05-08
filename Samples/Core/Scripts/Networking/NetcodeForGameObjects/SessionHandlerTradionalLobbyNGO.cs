using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HnSF.ui.menus;
using Photon.Realtime;
using Quantum;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HnSF.sessionhandling.handlers.NGO
{
    public partial class SessionHandlerTradionalLobbyNGO : SessionHandlerTraditionalLobby
    {
        public NetworkManager networkManagerPrefab;
        public NetworkManager networkManager;

        public NGOTraditionalLobbyNetworked lobbyNetworkedPrefab;
        public NGOTraditionalLobbyNetworked lobbyNetworkObject;

        public override async UniTask<bool> TryJoinLobby(string roomAddress)
        {
            if (networkManager == null) networkManager = Instantiate(networkManagerPrefab, null, false);
            DontDestroyOnLoad(networkManager);

            networkManager.GetComponent<UnityTransport>()
                .SetConnectionData(string.IsNullOrEmpty(roomAddress) ? "localhost" : roomAddress, (ushort)7777);
            bool clientResult = networkManager.StartClient();
            if (!clientResult) return false;

            await FindLobbyNetworkObject();
            await UniTask.WaitUntil(() => lobbyNetworkObject.localClientPlayerIds != null);
            localClientPlayerIds = lobbyNetworkObject.localClientPlayerIds;
            return true;
        }

        public override void SetConnectionData(string address, int port)
        {
            if (networkManager.TryGetComponent<UnityTransport>(out var unityTransport))
            {
                unityTransport.SetConnectionData(string.IsNullOrEmpty(address) ? "localhost" : address, (ushort)port);
            }
        }

        public override async UniTask<bool> TryCreateLobby()
        {
            void OnLocalClientPlayerIdsSet(NGOTraditionalLobbyNetworked arg0)
            {
                localClientPlayerIds = arg0.localClientPlayerIds;
            }

            if (networkManager == null) networkManager = Instantiate(networkManagerPrefab, null, false);
            DontDestroyOnLoad(networkManager);

            bool createResult = networkManager.StartHost();
            if (!createResult) return false;

            lobbyNetworkObject = Instantiate(lobbyNetworkedPrefab, null, false);
            lobbyNetworkObject.localClientPlayerCount = LocalPlayerCount;
            DontDestroyOnLoad(lobbyNetworkObject);
            lobbyNetworkObject.OnLocalClientPlayerIdsSet.AddListener(OnLocalClientPlayerIdsSet);
            lobbyNetworkObject.GetComponent<NetworkObject>().Spawn(destroyWithScene: false);
            RegisterForValueChanges();
            
            await UniTask.WaitUntil(() => lobbyNetworkObject.localClientPlayerIds != null);
            return true;
        }

        protected virtual async UniTask FindLobbyNetworkObject()
        {
            while (lobbyNetworkObject == null)
            {
                await UniTask.WaitForSeconds(0.5f);
                lobbyNetworkObject = GameObject.FindAnyObjectByType<NGOTraditionalLobbyNetworked>();
            }

            UpdateUIRepresentation();
            RegisterForValueChanges();
        }

        protected virtual void RegisterForValueChanges()
        {
            lobbyNetworkObject.OnLocalClientPlayerIdsSet.AddListener(UpdateUIRepresentation);
            lobbyNetworkObject.rooms.OnValueChanged += OnRoomListChanged;
            lobbyNetworkObject.players.OnValueChanged += OnPlayerListChanged;
        }

        public override void SetUiLobbyRepresentation(TraditionalLobbyUIRepresentation uiLobbyRepresentation)
        {
            base.SetUiLobbyRepresentation(uiLobbyRepresentation);
            UpdateUIRepresentation();
        }

        protected virtual void UpdateUIRepresentation(NGOTraditionalLobbyNetworked arg0)
        {
            UpdateUIRepresentation();
        }

        protected virtual void OnPlayerListChanged()
        {
            UpdateUIRepresentation();
            CheckRoomStatus();
        }

        protected virtual void OnRoomListChanged(List<LobbyRepresentation.Room> previousvalue,
            List<LobbyRepresentation.Room> newvalue)
        {
            UpdateUIRepresentation();
            CheckRoomStatus();
        }

        protected virtual void UpdateUIRepresentation()
        {
            if (uiLobbyRepresentation == null) return;

            UpdateUIRepresentationPlayers();
            UpdateUIRepresentationRooms();
        }

        protected virtual void UpdateUIRepresentationPlayers()
        {
            List<int> validPlayerIds = new List<int>();
            for (int i = 0; i < lobbyNetworkObject.players.Value.Count; i++)
            {
                var player = lobbyNetworkObject.players.Value[i];
                validPlayerIds.Add(player.playerId);
                if (uiLobbyRepresentation.ContainsPlayer(player.playerId))
                {
                    // TODO: Attempt to update player.
                    var uiPlayer = uiLobbyRepresentation.GetPlayer(player.playerId);

                    uiPlayer.playerId = player.playerId;
                    uiPlayer.playerName = player.playerName;
                    uiPlayer.ready = player.ready;
                    uiPlayer.selectedFighters = player.selectedFighters;
                    uiPlayer.selectedTeamId = (TeamBitmask)player.selectedTeam;

                    uiPlayer.onUpdated.Invoke(uiPlayer);
                    uiLobbyRepresentation.onPlayerUpdated.Invoke(uiPlayer.playerId);
                }
                else
                {
                    var uiPlayer = new TraditionalLobbyUIRepresentation.Player()
                    {
                        playerId = player.playerId,
                        playerName = player.playerName,
                        ready = player.ready,
                        roomId = player.roomId,
                        selectedFighters = player.selectedFighters,
                        selectedTeamId = (TeamBitmask)player.selectedTeam
                    };

                    uiLobbyRepresentation.players.Add(uiPlayer);
                    uiLobbyRepresentation.onPlayerEnterLobby.Invoke(uiPlayer.playerId);
                }
            }

            for (int i = uiLobbyRepresentation.players.Count - 1; i >= 0; i--)
            {
                var uiPlayer = uiLobbyRepresentation.players[i];
                if (validPlayerIds.Contains(uiPlayer.playerId)) continue;
                uiLobbyRepresentation.onPlayerLeaveLobby.Invoke(uiPlayer.playerId);
                uiLobbyRepresentation.players.RemoveAt(i);
            }
        }

        protected virtual void UpdateUIRepresentationRooms()
        {
            List<int> validRoomIds = new List<int>();
            for (int i = 0; i < lobbyNetworkObject.rooms.Value.Count; i++)
            {
                var room = lobbyNetworkObject.rooms.Value[i];
                validRoomIds.Add(room.roomId);
                if (uiLobbyRepresentation.ContainsRoom(room.roomId))
                {
                    var uiRoom = uiLobbyRepresentation.GetRoom(room.roomId);
                    bool updatedGamemode = room.selectedGamemode != uiRoom.selectedGamemode;
                    bool updatedMap = room.selectedMap != uiRoom.selectedMap;
                    // Attempt to update room info.
                    if (room.roomId != uiRoom.roomId || room.roomTitle != uiRoom.roomTitle ||
                        room.matchInProgressCode != uiRoom.matchInProgressCode
                        || room.selectedGamemode != uiRoom.selectedGamemode || room.selectedMap != uiRoom.selectedMap
                        || room.minimumPlayers != uiRoom.minimumPlayers || room.maximumPlayers != uiRoom.maximumPlayers
                        || RoomPlayerListAreDifferent(uiRoom.players, ref room.players) || room.status != uiRoom.status)
                    {
                        uiRoom.roomId = room.roomId;
                        uiRoom.roomTitle = room.roomTitle;
                        uiRoom.matchInProgressCode = room.matchInProgressCode;
                        uiRoom.selectedGamemode = room.selectedGamemode;
                        uiRoom.selectedMap = room.selectedMap;
                        uiRoom.minimumPlayers = room.minimumPlayers;
                        uiRoom.maximumPlayers = room.maximumPlayers;
                        uiRoom.gamemodeSettings = room.gamemodeSettings;
                        uiRoom.players = ConvertNativeArrayToList(ref room.players);
                        uiRoom.status = room.status;
                    }

                    bool RoomPlayerListAreDifferent(List<int> uiRoomPlayers, ref NativeList<int> roomPlayers)
                    {
                        if (uiRoomPlayers.Count != roomPlayers.Length) return true;
                        for (int n = 0; n < uiRoomPlayers.Count; n++)
                        {
                            if (uiRoomPlayers[n] != roomPlayers[n]) return true;
                        }

                        return false;
                    }

                    if (updatedGamemode) uiRoom.onGamemodeUpdated.Invoke(uiRoom);
                    if (updatedMap) uiRoom.onMapUpdated.Invoke(uiRoom);
                    uiRoom.onUpdated.Invoke(uiRoom);
                    uiLobbyRepresentation.onRoomUpdated.Invoke(room.roomId);
                }
                else
                {
                    // Create room in UI representation.
                    var uiRoom = new TraditionalLobbyUIRepresentation.Room()
                    {
                        roomId = room.roomId,
                        roomTitle = room.roomTitle,
                        matchInProgressCode = room.matchInProgressCode,
                        selectedGamemode = room.selectedGamemode,
                        selectedMap = room.selectedMap,
                        minimumPlayers = room.minimumPlayers,
                        maximumPlayers = room.maximumPlayers,
                        gamemodeSettings = room.gamemodeSettings,
                        players = ConvertNativeArrayToList(ref room.players),
                        status = room.status
                    };

                    uiLobbyRepresentation.rooms.Add(uiRoom);
                    uiLobbyRepresentation.onRoomOpened.Invoke(uiRoom.roomId);
                }
            }

            for (int i = uiLobbyRepresentation.rooms.Count - 1; i >= 0; i--)
            {
                if (validRoomIds.Contains(uiLobbyRepresentation.rooms[i].roomId)) continue;
                uiLobbyRepresentation.rooms[i].onClosed.Invoke(uiLobbyRepresentation.rooms[i]);
                uiLobbyRepresentation.onRoomClosed.Invoke(uiLobbyRepresentation.rooms[i].roomId);
                uiLobbyRepresentation.rooms.RemoveAt(i);
            }
        }

        protected LobbyRoomStatus lastKnownRoomStatus = LobbyRoomStatus.WaitingForPlayers;
        protected virtual void CheckRoomStatus()
        {
            if (localClientPlayerIds is not { Length: > 0 }) return;

            for (int i = 0; i < lobbyNetworkObject.rooms.Value.Count; i++)
            {
                var room = lobbyNetworkObject.rooms.Value[i];
                if (room.players.Contains(localClientPlayerIds[0]) == false) continue;
                if (room.status == lastKnownRoomStatus) return;
                
                switch (room.status)
                {
                    case LobbyRoomStatus.AwaitingMatchCode:
                        if (room.GetRoomMasterPlayerId() == localClientPlayerIds[0] && roomMatchSessionHandler == null)
                        {
                            InitializeRoomMatchSessionHandler();
                            roomMatchSessionHandler.Initialize();
                            roomMatchSessionHandler.matchCode = string.Empty;
                            roomMatchSessionHandler.OnRoomCreated.AddListener(RoomMasterSendMatchCode);
                            roomMatchSessionHandler.OnMatchEnded.AddListener(RoomMatchEnded);
                            _ = PrepareForMatchAndJoin(room);
                        }

                        break;
                    case LobbyRoomStatus.MatchInProgress:
                        if (room.players.Contains(localClientPlayerIds[0]) && roomMatchSessionHandler == null)
                        {
                            InitializeRoomMatchSessionHandler();
                            roomMatchSessionHandler.Initialize();
                            roomMatchSessionHandler.matchCode = room.matchInProgressCode;
                            roomMatchSessionHandler.OnMatchEnded.AddListener(RoomMatchEnded);
                            _ = PrepareForMatchAndJoin(room);
                        }

                        break;
                    case LobbyRoomStatus.WaitingForPlayers:
                        if (roomMatchSessionHandler != null)
                        {
                            roomMatchSessionHandler.ForceQuit();
                            Destroy(roomMatchSessionHandler.gameObject);
                            RoomMatchEnded(null);
                            roomMatchSessionHandler = null;
                        }
                        break;
                }
                
                lastKnownRoomStatus = room.status;
            }
        }
        
        protected virtual async UniTaskVoid PrepareForMatchAndJoin(LobbyRepresentation.Room room)
        {
            List<PlayerMatchContentBundle> localPlayerContentBundles = new List<PlayerMatchContentBundle>();
            
            for (int w = 0; w < localClientPlayerIds.Length; w++)
            {
                var bundle = new PlayerMatchContentBundle();
                var pIndex = lobbyNetworkObject.players.Value.FindIndex(x => x.playerId == localClientPlayerIds[w]);
                if (pIndex < 0)
                {
                    localPlayerContentBundles.Add(bundle);
                    continue;
                }

                var playerInfo = lobbyNetworkObject.players.Value[pIndex];
                
                await bundle.Create(new []{ playerInfo.selectedFighters[0] }, (TeamBitmask)playerInfo.selectedTeam);
                localPlayerContentBundles.Add(bundle);
            }

            _ = roomMatchSessionHandler.PrepareForMatchAndJoin(
                new QuantumMatchContentBundle()
                {
                    gamemodeReference = new ModAssetSoftReference(room.selectedGamemode),
                    mapReference = new ModAssetSoftReference(room.selectedMap),
                    musicReference = default,
                    gamemodeSettings = room.gamemodeSettings,
                    clientCount = lobbyNetworkObject.GetRoomClientCount(room.roomId),
                    playerCount = room.players.Length,
                    localPlayerBundles = localPlayerContentBundles
                });
        }

        protected virtual void RoomMatchEnded(string arg0)
        {
            roomMatchSessionHandler?.OnMatchEnded.RemoveListener(RoomMatchEnded);
            ReportMatchEndReason(MatchEndResult.Ended);
            WhenRoomMatchEnded();
        }

        protected virtual void WhenRoomMatchEnded()
        {
            
        }
        
        protected virtual void RoomMasterSendMatchCode(Room arg0)
        {
            lobbyNetworkObject.SendRoomMatchCodeRpc(networkManager.LocalClient.ClientId, arg0.Name);
        }

        protected virtual List<int> ConvertNativeArrayToList(ref NativeList<int> roomPlayers)
        {
            var l = new List<int>();

            for (int i = 0; i < roomPlayers.Length; i++)
            {
                l.Add(roomPlayers[i]);
            }

            return l;
        }

        public override async UniTask<int> CreateRoom(string title, int minimumPlayers, int maximumPlayers,
            LoadedAssetHandleWrapper gamemodeHandle, string gamemodeSettingsAsJson, LoadedAssetHandleWrapper mapHandle)
        {
            if (lobbyNetworkObject == null) return -1;

            int? gottenRoomId = null;

            void GotCreateRoomResult(int roomId)
            {
                gottenRoomId = roomId;
            }

            lobbyNetworkObject.OnCreateRoomResult.AddListener(GotCreateRoomResult);

            lobbyNetworkObject.AttemptCreateRoomRpc(networkManager.LocalClient.ClientId, new RoomUpdateInfo()
            {
                title = string.IsNullOrEmpty(title) ? $"Random Room {Random.Range(0, 10000)}" : title,
                minimumPlayers = Mathf.Max(2, minimumPlayers),
                maximumPlayers = Mathf.Max(minimumPlayers, maximumPlayers),
                gamemode = gamemodeHandle.assetReference.ToString(),
                gamemodeSettings = string.IsNullOrEmpty(gamemodeSettingsAsJson) ? string.Empty : gamemodeSettingsAsJson,
                map = mapHandle.assetReference.ToString()
            });

            await UniTask.WaitUntil(() => gottenRoomId.HasValue);
            lobbyNetworkObject.OnCreateRoomResult.RemoveListener(GotCreateRoomResult);

            if (gottenRoomId.HasValue == false || gottenRoomId.Value == -1) return -1;
            return gottenRoomId.Value;
        }

        public override async UniTask<bool> TryJoinRoom(int roomId)
        {
            if (lobbyNetworkObject == null) return false;
            int? gotRoomJoinResult = null;

            void GotJoinRoomResult(int gotRoomId)
            {
                gotRoomJoinResult = gotRoomId;
            }

            lobbyNetworkObject.OnJoinedRoomResult.AddListener(GotJoinRoomResult);

            lobbyNetworkObject.AttemptJoinRoomRpc(networkManager.LocalClient.ClientId, roomId);
            await UniTask.WaitUntil(() => gotRoomJoinResult.HasValue);
            lobbyNetworkObject.OnJoinedRoomResult.RemoveListener(GotJoinRoomResult);

            return gotRoomJoinResult.HasValue && gotRoomJoinResult.Value > 0;
        }

        public override void LeaveRoom()
        {
            if (lobbyNetworkObject == null) return;

            lobbyNetworkObject.AttemptLeaveRoomRpc(networkManager.LocalClient.ClientId);
        }

        public override async UniTask<bool> ChangeRoomGamemode(ModAssetSoftReference gamemodeReference)
        {
            if (lobbyNetworkObject == null) return false;

            bool? gotResult = null;

            void GotResult(bool result)
            {
                gotResult = result;
            }

            lobbyNetworkObject.OnChangeRoomGamemodeResult.AddListener(GotResult);
            lobbyNetworkObject.AttemptChangeRoomGamemodeRpc(networkManager.LocalClient.ClientId,
                gamemodeReference.ToString());
            await UniTask.WaitUntil(() => gotResult.HasValue);
            lobbyNetworkObject.OnChangeRoomGamemodeResult.RemoveListener(GotResult);

            return gotResult.HasValue && gotResult.Value;
        }

        public override async UniTask<bool> ChangeRoomGamemodeSettings(string gamemodeSettingsAsJson)
        {
            if (lobbyNetworkObject == null) return false;

            bool? gotResult = null;

            void GotResult(bool result)
            {
                gotResult = result;
            }

            lobbyNetworkObject.OnChangeRoomGamemodeSettingsResult.AddListener(GotResult);
            lobbyNetworkObject.AttemptChangeRoomGamemodeSettingsRpc(networkManager.LocalClient.ClientId,
                gamemodeSettingsAsJson);
            await UniTask.WaitUntil(() => gotResult.HasValue);
            lobbyNetworkObject.OnChangeRoomGamemodeSettingsResult.RemoveListener(GotResult);

            return gotResult.HasValue && gotResult.Value;
        }

        public override async UniTask<bool> ChangeRoomMap(ModAssetSoftReference mapReference)
        {
            if (lobbyNetworkObject == null) return false;

            bool? gotResult = null;

            void GotResult(bool result)
            {
                gotResult = result;
            }

            lobbyNetworkObject.OnChangeRoomMapResult.AddListener(GotResult);
            lobbyNetworkObject.AttemptChangeRoomMapRpc(networkManager.LocalClient.ClientId, mapReference.ToString());
            await UniTask.WaitUntil(() => gotResult.HasValue);
            lobbyNetworkObject.OnChangeRoomMapResult.RemoveListener(GotResult);

            return gotResult.HasValue && gotResult.Value;
        }

        public override UniTask<bool> ChangePlayerFighters(int localPlayer,
            ModAssetSoftReference[] fighterReferences)
        {
            if (lobbyNetworkObject == null || fighterReferences == null) return new UniTask<bool>(false);

            var fighterRefsAsStr = new string[fighterReferences.Length];
            for (int i = 0; i < fighterRefsAsStr.Length; i++) fighterRefsAsStr[i] = fighterReferences[i].ToString();

            lobbyNetworkObject.AttemptSetPlayerFightersRpc(networkManager.LocalClient.ClientId,
                localClientPlayerIds[localPlayer], new NetworkStringArray() { Array = fighterRefsAsStr });

            return new UniTask<bool>(true);
        }

        public override UniTask<bool> ChangePlayerTeam(int localPlayer, TeamBitmask team)
        {
            if (lobbyNetworkObject == null) return new UniTask<bool>(false);
            
            lobbyNetworkObject.AttemptSetPlayerTeamRpc(networkManager.LocalClient.ClientId,
                localClientPlayerIds[localPlayer], team);

            return new UniTask<bool>(true);
        }

        public override UniTask<bool> AttemptToggleReadyState(int localPlayer)
        {
            if (lobbyNetworkObject == null) return new UniTask<bool>(false);

            lobbyNetworkObject.AttemptTogglePlayerReadyStateRpc(networkManager.LocalClient.ClientId,
                localClientPlayerIds[localPlayer]);

            return new UniTask<bool>(true);
        }

        public override void ReportMatchEndReason(MatchEndResult reason)
        {
            lobbyNetworkObject.SendMatchResultRpc(networkManager.LocalClient.ClientId, reason);
        }
    }
}

