using System.Collections.Generic;
using System.Linq;
using Quantum;
using Unity.Collections;
using UnityEngine;

namespace HnSF.sessionhandling.handlers.NGO
{
    public partial class NGOTraditionalLobbyNetworked
    {
        public int GetRoomIndex(int roomId)
        {
            for (int i = 0; i < rooms.Value.Count; i++)
            {
                if (rooms.Value[i].roomId == roomId) return i;
            }
            return -1;
        }
        
        public bool PlayerInAnyRoom(int playerId)
        {
            for (int i = 0; i < rooms.Value.Count; i++)
            {
                if (rooms.Value[i].players.Contains(playerId)) return true;
            }
            return false;
        }

        public int GetRoomPlayerIsInIndex(int playerId)
        {
            for (int i = 0; i < rooms.Value.Count; i++)
            {
                if (rooms.Value[i].players.Contains(playerId)) return i;
            }
            return -1;
        }

        public int GetRoomPlayerIsInId(int playerId)
        {
            for (int i = 0; i < rooms.Value.Count; i++)
            {
                if (rooms.Value[i].players.Contains(playerId)) return rooms.Value[i].roomId;
            }
            return 0;
        }

        public int[] GetClientPlayerIds(ulong clientId)
        {
            List<int> clientPlayerIds = new List<int>();

            for (int i = 0; i < players.Value.Count; i++)
            {
                if (players.Value[i].clientId != clientId) continue;
                clientPlayerIds.Add(players.Value[i].playerId);
            }
            clientPlayerIds.Sort();
            return clientPlayerIds.ToArray();
        }
        
        public int GetRoomClientIsInIndex(ulong clientId)
        {
            var clientPlayerIds = GetClientPlayerIds(clientId);
            if (clientPlayerIds.Length == 0) return -1;

            for (int i = 0; i < rooms.Value.Count; i++)
            {
                if (rooms.Value[i].players.Contains(clientPlayerIds[0])) return i;
            }
            return -1;
        }

        public int GetRoomClientCount(int roomId)
        {
            var roomIndex = GetRoomIndex(roomId);
            if (roomIndex == -1) return -1;
            var room = rooms.Value[roomIndex];
            if(room.players.Length <= 1) return room.players.Length;
            List<ulong> foundClients = new List<ulong>();
            int cnt = 0;
            for (int i = 0; i < room.players.Length; i++)
            {
                var playerIndex = GetPlayerIndex(room.players[i]);
                if (playerIndex == -1) continue;
                var player = players.Value[playerIndex];
                if (foundClients.Contains(player.clientId)) continue;
                cnt++;
                foundClients.Add(player.clientId);
            }

            return cnt;
        }

        public int GetPlayerIndexInRoom(int playerId)
        {
            var roomIndex = GetRoomPlayerIsInIndex(playerId);
            if (roomIndex == -1) return -1;
            return GetPlayerIndexInRoom(roomIndex, playerId);
        }

        private int GetPlayerIndexInRoom(int roomIndex, int playerId)
        {
            return rooms.Value[roomIndex].players.IndexOf(playerId);
        }

        private int GetPlayerIndex(int playerId)
        {
            var player = players.Value.First(x => x.playerId == playerId);
            return player.playerId == 0 ? -1 : players.Value.IndexOf(player);
        }

        private void RemovePlayerFromRoom(int playerId, bool checkDirtyState = true)
        {
            var roomIndex = GetRoomPlayerIsInIndex(playerId);
            if (roomIndex == -1) return;
            rooms.Value[roomIndex].players.RemoveAt(GetPlayerIndexInRoom(roomIndex, playerId));
            var playerIndex = GetPlayerIndex(playerId);
            if (playerIndex != -1)
            {
                var player = players.Value[playerIndex];
                player.roomId = 0;
                player.ready = false;
                players.Value[playerIndex] = player;
                players.CheckDirtyState();
            }
            if (checkDirtyState) rooms.CheckDirtyState();
        }

        private void RemoveClientFromRoom(ulong clientId, bool checkDirtyState = true)
        {
            var clientPlayerIds = GetClientPlayerIds(clientId);
            if(clientPlayerIds.Length == 0) return;
            var roomIndex = GetRoomClientIsInIndex(clientId);
            if (roomIndex == -1) return;
            
            foreach(var playerId in clientPlayerIds) rooms.Value[roomIndex].players.RemoveAt(GetPlayerIndexInRoom(roomIndex, playerId));

            if (checkDirtyState) rooms.CheckDirtyState();
        }

        private bool AttemptJoinRoom(ulong clientId, int roomId, bool checkDirtyState = true)
        {
            var roomIndex = GetRoomIndex(roomId);
            if (roomIndex == -1) return false;
            if (rooms.Value[roomIndex].MatchInProgress()) return false;
            var clientPlayerIds = GetClientPlayerIds(clientId);
            if(clientPlayerIds.Length == 0) return false;
            if (!rooms.Value[roomIndex].CanFitPlayerCount(clientPlayerIds.Length)) return false;

            var forceDirty = false;

            foreach (var clientPlayerId in clientPlayerIds)
            {
                RemovePlayerFromRoom(clientPlayerId, false);
                forceDirty = true;
            }

            foreach (var clientPlayerId in clientPlayerIds)
            {
                rooms.Value[roomIndex].players.Add(clientPlayerId);
                forceDirty = true;
            }

            if (forceDirty)
            {
                var r = rooms.Value[roomIndex];
                r.garbage += 1;
                rooms.Value[roomIndex] = r;
            }

            if (checkDirtyState) rooms.CheckDirtyState(true);
            return true;
        }

        private bool AttemptChangeRoomGamemode(ulong clientId, string selectedGamemode)
        {
            var clientPlayerIds = GetClientPlayerIds(clientId);
            if(clientPlayerIds.Length == 0) return false;
            var roomIndex = GetRoomClientIsInIndex(clientId);
            if (roomIndex == -1) return false;
            if(rooms.Value[roomIndex].GetRoomMasterPlayerId() != clientPlayerIds[0]) return false;

            var room = rooms.Value[roomIndex];
            if (room.selectedGamemode != selectedGamemode) room.gamemodeSettings = string.Empty;
            room.selectedGamemode = selectedGamemode;
            rooms.Value[roomIndex] = room;

            rooms.CheckDirtyState();
            return true;
        }

        private bool AttemptChangeRoomGamemodeSettings(ulong clientId, string gamemodeSettings)
        {
            var clientPlayerIds = GetClientPlayerIds(clientId);
            if(clientPlayerIds.Length == 0) return false;
            var roomIndex = GetRoomClientIsInIndex(clientId);
            if (roomIndex == -1) return false;
            if(rooms.Value[roomIndex].GetRoomMasterPlayerId() != clientPlayerIds[0]) return false;

            var room = rooms.Value[roomIndex];
            room.gamemodeSettings = gamemodeSettings;
            rooms.Value[roomIndex] = room;

            rooms.CheckDirtyState();
            return true;
        }

        private bool AttemptChangeRoomMap(ulong clientId, string selectedMap)
        {
            var clientPlayerIds = GetClientPlayerIds(clientId);
            if(clientPlayerIds.Length == 0) return false;
            var roomIndex = GetRoomClientIsInIndex(clientId);
            if (roomIndex == -1) return false;
            if(rooms.Value[roomIndex].GetRoomMasterPlayerId() != clientPlayerIds[0]) return false;

            var room = rooms.Value[roomIndex];
            room.selectedMap = selectedMap;
            rooms.Value[roomIndex] = room;

            rooms.CheckDirtyState();
            return true;
        }

        private bool AttemptSetPlayerFighters(int playerId, string[] selectedFighters)
        {
            var playerIndex = GetPlayerIndex(playerId);
            if (playerIndex == -1) return false;
            if (selectedFighters == null) return false;

            var pTemp = players.Value[playerIndex];
            pTemp.SetFighters(selectedFighters);
            players.Value[playerIndex] = pTemp;

            players.CheckDirtyState();
            return true;
        }
        
        private bool AttemptSetPlayerTeam(int playerId, TeamBitmask selectedTeam)
        {
            var playerIndex = GetPlayerIndex(playerId);
            if (playerIndex == -1) return false;

            var pTemp = players.Value[playerIndex];
            pTemp.selectedTeam = (int)selectedTeam;
            players.Value[playerIndex] = pTemp;
            
            players.CheckDirtyState();
            return true;
        }

        private bool TogglePlayerReadyState(int playerId)
        {
            var playerIndex = GetPlayerIndex(playerId);
            if (playerId == -1) return false;
            if (PlayerInAnyRoom(playerId) == false) return false;

            var player = players.Value[playerIndex];
            player.ready = !player.ready;
            players.Value[playerIndex] = player;

            players.CheckDirtyState();
            return true;
        }

        private bool AttemptStartRoomCountdown(int roomId)
        {
            var roomIndex = GetRoomIndex(roomId);
            if (roomIndex == -1) return false;
            var room = rooms.Value[roomIndex];
            
            if (room.status == LobbyRoomStatus.MatchInProgress || room.status == LobbyRoomStatus.AwaitingMatchCode || room.players.Length < room.minimumPlayers || room.players.Length > room.maximumPlayers) return false;
            for (int i = 0; i < room.players.Length; i++)
            {
                var playerIndex = GetPlayerIndex(room.players[i]);
                if(playerIndex == -1) continue;
                var player = players.Value[playerIndex];
                if (player.ready == false)
                {
                    room.status = LobbyRoomStatus.WaitingForPlayers;
                    room.startMatchTime = 0;
                    rooms.Value[roomIndex] = room;
                    rooms.CheckDirtyState();
                    return false;
                }
            }
            if (room.status == LobbyRoomStatus.CountingDown || room.status == LobbyRoomStatus.AwaitingMatchCode) return true;
            Debug.Log("All players ready, counting down.");
            room.status = LobbyRoomStatus.CountingDown;
            room.startMatchTime = NetworkManager.ServerTime.Time + 5;
            rooms.Value[roomIndex] = room;
            rooms.CheckDirtyState();
            return true;
        }
    }
}