using System;
using System.Collections.Generic;
using CT.LocalInputManagement;
using Quantum;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace HnSF.sessionhandling.handlers.NGO
{
    public partial class NGOTraditionalLobbyNetworked : NetworkBehaviour
    {
        public UnityEvent<int> OnCreateRoomResult = new();
        public UnityEvent<int> OnJoinedRoomResult = new();
        public UnityEvent<bool> OnChangeRoomGamemodeResult = new();
        public UnityEvent<bool> OnChangeRoomGamemodeSettingsResult = new();
        public UnityEvent<bool> OnChangeRoomMapResult = new();
        
        public UnityEvent<NGOTraditionalLobbyNetworked> OnLocalClientPlayerIdsSet = new();

        public NetworkVariable<List<SessionHandlerTradionalLobbyNGO.LobbyRepresentation.Room>> rooms = new();

        public NetworkVariableRoomPlayerList players = new();
        
        public HashSet<ulong> registeredPlayers = new HashSet<ulong>();

        public int roomIdCounter = 0;
        public int playerIdCounter = 0;
        
        [NonSerialized] public int[] localClientPlayerIds = null;

        public NGOClientSecretHandler secretHandler;
        
        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha7)) PrintAllLobbies();
        }

        private void PrintAllLobbies()
        {
            var lobbiesString = "";

            for (int i = 0; i < rooms.Value.Count; i++)
            {
                lobbiesString += $"ROOM {rooms.Value[i].roomTitle} [{rooms.Value[i].roomId}]\n";
                lobbiesString += $"--- Players: ";
                for (int w = 0; w < rooms.Value[i].players.Length; w++)
                {
                    lobbiesString += $"{rooms.Value[i].players[w]},";
                }
            }
            Debug.Log(lobbiesString);
        }

        public override void OnNetworkSpawn()
        {
            rooms.OnValueChanged += OnRoomListChanged;
            players.OnValueChanged += OnPlayerListChanged;
            NetworkManager.OnConnectionEvent += OnConnectionEvent;
            NetworkManager.NetworkTickSystem.Tick += Tick;

            if (NetworkManager.IsHost)
            {
                RegisterClientPlayersRpc(NetworkManager.LocalClient.ClientId, InputManagerBase.instance.GetPlayerCount(), $"Hosting Player");
            }
        }

        private int delayedTicks = 0;
        private void Tick()
        {
            if (NetworkManager.IsHost)
            {
                delayedTicks++;
                if (delayedTicks < 30) return;
                delayedTicks = 0;
                
                for (int i = 0; i < rooms.Value.Count; i++)
                {
                    var room = rooms.Value[i];
                    
                    switch (room.status)
                    {
                        case LobbyRoomStatus.CountingDown:
                            if(NetworkManager.ServerTime.Time < room.startMatchTime) continue;
                            room.status = LobbyRoomStatus.AwaitingMatchCode;
                            rooms.Value[i] = room;
                            rooms.CheckDirtyState(true);
                            break;
                        case LobbyRoomStatus.AwaitingMatchCode:
                            break;
                        case LobbyRoomStatus.MatchInProgress:
                            CheckForRoomMatchOver(i);
                            break;
                    }
                }
            }
        }

        private HashSet<ulong> matchOverCheckedClientIdsMatchEnd = new();
        private HashSet<ulong> matchOverCheckedClientIdsInMatch = new();
        private void CheckForRoomMatchOver(int roomIndex)
        {
            var room = rooms.Value[roomIndex];
            
            matchOverCheckedClientIdsMatchEnd.Clear();
            matchOverCheckedClientIdsInMatch.Clear();
            for (int i = 0; i < room.players.Length; i++)
            {
                var playerIndex = GetPlayerIndex(room.players[i]);
                if (playerIndex == -1) continue;
                var player = players.Value[playerIndex];
                if (player.matchResult == MatchEndResult.Quit) continue;
                matchOverCheckedClientIdsInMatch.Add(player.clientId);
                if (player.matchResult == MatchEndResult.None) continue;
                if(player.matchResult != MatchEndResult.Ended) continue;
                matchOverCheckedClientIdsMatchEnd.Add(player.clientId);
            }

            int clientsNeededToResetLobby = Mathf.Clamp(matchOverCheckedClientIdsInMatch.Count / 2, 1, int.MaxValue);
            if (matchOverCheckedClientIdsMatchEnd.Count < clientsNeededToResetLobby) return;

            room.matchInProgressCode = string.Empty;
            room.status = LobbyRoomStatus.WaitingForPlayers;
            room.startMatchTime = 0;

            for (int i = 0; i < room.players.Length; i++)
            {
                var playerIndex = GetPlayerIndex(room.players[i]);
                if (playerIndex == -1) continue;
                var player = players.Value[playerIndex];
                player.ready = false;
                player.matchResult = MatchEndResult.None;
                players.Value[playerIndex] = player;
            }

            rooms.Value[roomIndex] = room;
            
            players.CheckDirtyState();
            rooms.CheckDirtyState();
        }

        private void OnConnectionEvent(NetworkManager networkManager, ConnectionEventData connectionEventData)
        {
            if (networkManager.ShutdownInProgress) return;
            Debug.Log($"Client connection, {connectionEventData.ClientId} [{connectionEventData.EventType}]");
            switch (connectionEventData.EventType)
            {
                case ConnectionEvent.ClientConnected:
                    if (connectionEventData.ClientId == networkManager.LocalClient.ClientId)
                    {
                        RegisterClientPlayersRpc(networkManager.LocalClient.ClientId, InputManagerBase.instance.GetPlayerCount(), $"{Random.Range(0, 1000)}");
                    }
                    break;
                case ConnectionEvent.ClientDisconnected:
                    if(networkManager.IsServer) RemoveClientFromLobby(connectionEventData.ClientId);
                    break;
            }
        }

        public override void OnNetworkDespawn()
        {
            NetworkManager.NetworkTickSystem.Tick -= Tick;
            rooms.OnValueChanged -= OnRoomListChanged;
            players.OnValueChanged -= OnPlayerListChanged;
            NetworkManager.OnConnectionEvent -= OnConnectionEvent;
            for (int i = 0; i < rooms.Value.Count; i++)
            {
                if(rooms.Value[i].players.IsCreated) rooms.Value[i].players.Dispose();
            }
        }

        private void RemoveClientFromLobby(ulong clientId)
        {
            var clientPlayerIds = new List<int>();
            for (int i = 0; i < players.Value.Count; i++)
            {
                if(players.Value[i].clientId == clientId) clientPlayerIds.Add(players.Value[i].playerId);
            }

            bool roomsChanged = false;
            for (int i = 0; i < rooms.Value.Count; i++)
            {
                for (int j = 0; j < clientPlayerIds.Count; j++)
                {
                    if (!rooms.Value[i].players.Contains(clientPlayerIds[j])) continue;
                    rooms.Value[i].players.RemoveAt( rooms.Value[i].players.IndexOf(clientPlayerIds[j]) );
                    roomsChanged = true;
                }
            }

            for (int i = players.Value.Count-1; i >= 0; i--)
            {
                if (players.Value[i].clientId == clientId) players.Value.RemoveAt(i);
            }
            
            if (roomsChanged) rooms.CheckDirtyState();
            players.CheckDirtyState();
        }
        
        private void OnRoomListChanged(List<SessionHandlerTradionalLobbyNGO.LobbyRepresentation.Room> previousvalue, List<SessionHandlerTradionalLobbyNGO.LobbyRepresentation.Room> newvalue)
        {
            
        }
        
        private void OnPlayerListChanged()
        {
        }

        [Rpc(SendTo.Server)]
        public void RegisterClientPlayersRpc(ulong clientId, int localPlayerCount, string playerName)
        {
            if (!NetworkManager.ConnectedClients.ContainsKey(clientId) || registeredPlayers.Contains(clientId) ||
                localPlayerCount <= 0 || localPlayerCount > 4 || string.IsNullOrEmpty(playerName))
            {
                return;
            }
            registeredPlayers.Add(clientId);

            var clientPlayerIds = new List<int>();
            
            for (int i = 0; i < localPlayerCount; i++)
            {
                var player = new SessionHandlerTradionalLobbyNGO.LobbyRepresentation.Player(clientId, i, ++playerIdCounter, i == 0 ? playerName : $"{playerName} ({i})", 0, 0);
                players.Value.Add(player);
                clientPlayerIds.Add(player.playerId);
            }
            players.CheckDirtyState();

            SendClientPlayerIdsRpc(clientPlayerIds.ToArray(), RpcTarget.Single(clientId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.SpecifiedInParams)]
        public void SendClientPlayerIdsRpc(int[] playerIds, RpcParams rpcParams = default)
        {
            localClientPlayerIds = playerIds;
            OnLocalClientPlayerIdsSet.Invoke(this);
        }

        [Rpc(SendTo.Server)]
        public void AttemptCreateRoomRpc(ulong clientId, RoomUpdateInfo initialRoomInfo)
        {
            if (!NetworkManager.ConnectedClients.ContainsKey(clientId) || initialRoomInfo.Equals(default) || !initialRoomInfo.IsValidRoom())
            {
                AttemptCreateRoomResultRpc(0, RpcTarget.Single(clientId, RpcTargetUse.Temp));
                return;
            }

            var room = initialRoomInfo.GenerateRoom();
            room.roomId = ++roomIdCounter;
            rooms.Value.Add(room);

            var joinResult = AttemptJoinRoom(clientId, room.roomId, checkDirtyState: true);
            AttemptJoinRoomResultRpc(room.roomId, RpcTarget.Single(clientId, RpcTargetUse.Temp));
            AttemptCreateRoomResultRpc(room.roomId, RpcTarget.Single(clientId, RpcTargetUse.Temp));
        }
        
        [Rpc(SendTo.SpecifiedInParams)]
        public void AttemptCreateRoomResultRpc(int roomId, RpcParams rpcParams = default)
        {
            Debug.Log(roomId <= 0 ? "Failed to create room." : $"Created room of id {roomId}");
            OnCreateRoomResult.Invoke(roomId);
        }

        [Rpc(SendTo.Server)]
        public void AttemptJoinRoomRpc(ulong clientId, int roomId)
        {
            if (!NetworkManager.ConnectedClients.ContainsKey(clientId))
            {
                AttemptJoinRoomResultRpc(0, RpcTarget.Single(clientId, RpcTargetUse.Temp));
                return;
            }
            
            var joinResult = AttemptJoinRoom(clientId, roomId, checkDirtyState: true);
            AttemptJoinRoomResultRpc(joinResult ? roomId : 0, RpcTarget.Single(clientId, RpcTargetUse.Temp));
        }
        
        [Rpc(SendTo.SpecifiedInParams)]
        public void AttemptJoinRoomResultRpc(int roomId, RpcParams rpcParams = default)
        {
            Debug.Log(roomId <= 0 ? "Failed to join room." : $"Joined room of id {roomId}");
            OnJoinedRoomResult.Invoke(roomId);
        }
        
        [Rpc(SendTo.Server)]
        public void AttemptForceStartRoomRpc()
        {
            
        }

        [Rpc(SendTo.Server)]
        public void AttemptLeaveRoomRpc(ulong clientId)
        {
            if (!NetworkManager.ConnectedClients.ContainsKey(clientId)) return;
            
            RemoveClientFromRoom(clientId, checkDirtyState: true);
        }

        [Rpc(SendTo.Server)]
        public void AttemptChangeRoomGamemodeRpc(ulong clientId, string gamemode)
        {
            if (!NetworkManager.ConnectedClients.ContainsKey(clientId)) return;
            
            bool result = AttemptChangeRoomGamemode(clientId, gamemode);
            AttemptChangeRoomGamemodeResultRpc(result, RpcTarget.Single(clientId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.SpecifiedInParams)]
        public void AttemptChangeRoomGamemodeResultRpc(bool result, RpcParams rpcParams = default)
        {
            OnChangeRoomGamemodeResult.Invoke(result);
        }
        
        [Rpc(SendTo.Server)]
        public void AttemptChangeRoomGamemodeSettingsRpc(ulong clientId, string gamemode)
        {
            if (!NetworkManager.ConnectedClients.ContainsKey(clientId)) return;
            
            bool result = AttemptChangeRoomGamemodeSettings(clientId, gamemode);
            AttemptChangeRoomGamemodeSettingsResultRpc(result, RpcTarget.Single(clientId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.SpecifiedInParams)]
        public void AttemptChangeRoomGamemodeSettingsResultRpc(bool result, RpcParams rpcParams = default)
        {
            OnChangeRoomGamemodeResult.Invoke(result);
        }
        
        [Rpc(SendTo.Server)]
        public void AttemptChangeRoomMapRpc(ulong clientId, string map)
        {
            if (!NetworkManager.ConnectedClients.ContainsKey(clientId)) return;
            
            bool result = AttemptChangeRoomMap(clientId, map);
            AttemptChangeRoomMapResultRpc(result, RpcTarget.Single(clientId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.SpecifiedInParams)]
        public void AttemptChangeRoomMapResultRpc(bool result, RpcParams rpcParams = default)
        {
            OnChangeRoomMapResult.Invoke(result);
        }
        
        [Rpc(SendTo.Server)]
        public void AttemptSetPlayerFightersRpc(ulong clientId, int playerId, NetworkStringArray fighters)
        {
            if (!NetworkManager.ConnectedClients.ContainsKey(clientId)) return;
            AttemptSetPlayerFighters(playerId, fighters.Array);
        }

        [Rpc(SendTo.Server)]
        public void AttemptSetPlayerTeamRpc(ulong clientId, int playerId, TeamBitmask team)
        {
            if (!NetworkManager.ConnectedClients.ContainsKey(clientId)) return;
            AttemptSetPlayerTeam(playerId, team);
        }
        
        [Rpc(SendTo.Server)]
        public void AttemptTogglePlayerReadyStateRpc(ulong clientId, int playerId)
        {
            if (!NetworkManager.ConnectedClients.ContainsKey(clientId)) return;
            
            var toggleResult = TogglePlayerReadyState(playerId);
            if (toggleResult == false) return;
            Debug.Log($"Toggled player {clientId} ready.");
            AttemptStartRoomCountdown(GetRoomPlayerIsInId(playerId));
        }
        
        [Rpc(SendTo.Server)]
        public void SendRoomMatchCodeRpc(ulong clientId, string matchCode)
        {
            if (!NetworkManager.ConnectedClients.ContainsKey(clientId)) return;
            if(string.IsNullOrEmpty(matchCode)) return;
            var roomIndex = GetRoomClientIsInIndex(clientId);
            if (roomIndex == -1) return;
            var room = rooms.Value[roomIndex];
            if (room.status != LobbyRoomStatus.AwaitingMatchCode) return;
            room.matchInProgressCode = matchCode;
            room.status = LobbyRoomStatus.MatchInProgress;
            rooms.Value[roomIndex] = room;
            rooms.CheckDirtyState();
        }
        
        [Rpc(SendTo.Server)]
        public void SendMatchResultRpc(ulong clientId, MatchEndResult result)
        {
            if (!NetworkManager.ConnectedClients.ContainsKey(clientId) || result == MatchEndResult.None) return;
            
            var roomIndex = GetRoomClientIsInIndex(clientId);
            if (roomIndex == -1) return;
            var room = rooms.Value[roomIndex];
            if (room.status != LobbyRoomStatus.MatchInProgress) return;

            var clientPlayerIds = GetClientPlayerIds(clientId);
            for (int i = 0; i < clientPlayerIds.Length; i++)
            {
                var playerIndex = GetPlayerIndex(clientPlayerIds[i]);
                if (playerIndex == -1) continue;
                var player = players.Value[playerIndex];
                player.matchResult = result;
                players.Value[playerIndex] = player;
            }
            players.CheckDirtyState();
        }
    }
}