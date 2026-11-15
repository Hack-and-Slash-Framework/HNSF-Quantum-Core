using System.Collections.Generic;
using HnSF.sessionhandling.handlers;
using Quantum;
using UnityEngine.Events;

namespace HnSF.ui.menus
{
    [System.Serializable]
    public class TraditionalLobbyUIRepresentation{
        [System.Serializable]
        public class Room
        {
            public static readonly int InvalidRoomId = 0;
            
            public UnityEvent<Room> onClosed = new UnityEvent<Room>();
            public UnityEvent<Room> onUpdated = new UnityEvent<Room>();
            public UnityEvent<Room> onGamemodeUpdated = new UnityEvent<Room>();
            public UnityEvent<Room> onMapUpdated = new UnityEvent<Room>();
            
            public int roomId;
            public string roomTitle;
            public string matchInProgressCode;

            public string gamemodeTitle;
            public string mapTitle;
            
            public string selectedGamemode;
            public string selectedMap;

            public int minimumPlayers;
            public int maximumPlayers;
            
            public string gamemodeSettings;

            public List<int> players = new List<int>();
            
            public LobbyRoomStatus status;
            
            public bool CanFitPlayerCount(int playerCount)
            {
                return (maximumPlayers - players.Count) >= playerCount;
            }
                
            public int GetRoomMasterPlayerId()
            {
                if (players.Count == 0) return -1;
                return players[0];
            }
                
            public bool MatchInProgress()
            {
                return string.IsNullOrEmpty(matchInProgressCode) == false;
            }

            public int GetPlayerFighterCount()
            {
                if (string.IsNullOrEmpty(gamemodeSettings)) return 0;
                GamemodeSettingsBase settings = JsonUtilityExtensions.FromJsonWithTypeAnnotation<GamemodeSettingsBase>(gamemodeSettings);
                if (settings == null) return 0;
                return settings.fightersPerPlayer;
            }
        }
        
        [System.Serializable]
        public class Player
        {
            public static readonly int InvalidPlayerId = 0;
            
            public UnityEvent<Player> onUpdated = new UnityEvent<Player>();
            
            public int playerId;
            public string playerName;
            
            public int roomId;
            public bool ready;
            
            public string[] selectedFighters = new string[4];
            public TeamBitmask selectedTeamId;
        }

        public UnityEvent<int> onRoomUpdated = new UnityEvent<int>();
        public UnityEvent<int> onRoomOpened = new UnityEvent<int>();
        public UnityEvent<int> onRoomClosed = new UnityEvent<int>();
        
        public UnityEvent<int> onPlayerEnterLobby = new UnityEvent<int>();
        public UnityEvent<int> onPlayerUpdated = new UnityEvent<int>();
        public UnityEvent<int> onPlayerLeaveLobby = new UnityEvent<int>();
        
        public List<Room> rooms = new List<Room>();
        public List<Player> players = new List<Player>();

        public bool ContainsPlayer(int playerId)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].playerId == playerId) return true;
            }
            return false;
        }

        public int IndexOfPlayerId(int playerId)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if(players[i].playerId == playerId) return i;
            }
            return -1;
        }

        public Player GetPlayer(int playerId)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].playerId == playerId) return players[i];
            }
            return null;
        }

        public bool ContainsRoom(int roomId)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                if (rooms[i].roomId == roomId) return true;
            }
            return false;
        }

        public int IndexOfRoomId(int roomId)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                if (rooms[i].roomId == roomId) return i;
            }
            return -1;
        }

        public Room GetRoom(int roomId)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                if (rooms[i].roomId == roomId) return rooms[i];
            }
            return default;
        }
    }
}

