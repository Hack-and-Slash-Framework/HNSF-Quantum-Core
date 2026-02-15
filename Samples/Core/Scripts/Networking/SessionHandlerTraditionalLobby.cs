using System;
using Cysharp.Threading.Tasks;
using HnSF.ui.menus;
using Quantum;
using UnityEngine;

namespace HnSF.sessionhandling.handlers
{
    public class SessionHandlerTraditionalLobby : SessionHandlerBase
    {
        [NonSerialized] public int[] localClientPlayerIds = null;

        [SerializeField] private SessionHandlerLobbyRoomMatch roomMatchSessionHandlerPrefab;
        public SessionHandlerLobbyRoomMatch roomMatchSessionHandler;
        
        public TraditionalLobbyUIRepresentation uiLobbyRepresentation;
        
        public void InitializeRoomMatchSessionHandler()
        {
            if (roomMatchSessionHandler != null) return;
            roomMatchSessionHandler = GameObject.Instantiate(roomMatchSessionHandlerPrefab, transform, false);
        }
        
        public virtual void SetUiLobbyRepresentation(TraditionalLobbyUIRepresentation lobbyRepresentation)
        {
            uiLobbyRepresentation = lobbyRepresentation;
        }
        
        public virtual UniTask<bool> TryJoinLobby(string roomAddress)
        {
            return new UniTask<bool>(false);
        }

        public virtual void SetConnectionData(string address, int port)
        {
            
        }

        public virtual UniTask<bool> TryCreateLobby()
        {
            return new UniTask<bool>(false);
        }
        
        public virtual UniTask<int> CreateRoom(string title, int minimumPlayers, int maximumPlayers, LoadedAssetHandleWrapper gamemodeHandle, string gamemodeSettingsAsJson, LoadedAssetHandleWrapper mapHandle)
        {
            return new UniTask<int>(-1);
        }

        public virtual UniTask<bool> TryJoinRoom(int roomId)
        {
            return new UniTask<bool>(false);
        }

        public virtual void LeaveRoom()
        {
            
        }

        public virtual UniTask<bool> ChangeRoomGamemode(ModAssetSoftReference gamemodeReference)
        {
            return new UniTask<bool>(false);
        }

        public virtual UniTask<bool> ChangeRoomGamemodeSettings(string gamemodeSettingsAsJson)
        {
            return new UniTask<bool>(false);
        }

        public virtual UniTask<bool> ChangeRoomMap(ModAssetSoftReference mapReference)
        {
            return new UniTask<bool>(false);
        }

        public virtual UniTask<bool> ChangePlayerFighters(int localPlayer, ModAssetSoftReference[] fighterReferences)
        {
            return new UniTask<bool>(false);
        }

        public virtual UniTask<bool> ChangePlayerTeam(int localPlayer, TeamBitmask team)
        {
            return new UniTask<bool>(false);
        }

        public virtual UniTask<bool> AttemptToggleReadyState(int localPlayer)
        {
            return new UniTask<bool>(false);
        }

        public virtual void ReportMatchEndReason(MatchEndResult reason)
        {
            
        }
    }
}
