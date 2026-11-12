using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace HnSF.sessionhandling.handlers.NGO
{
    public struct RoomUpdateInfo : INetworkSerializable, IEquatable<RoomUpdateInfo>
    {
        public string title;
        public string gamemode;
        public string map;
        public int minimumPlayers;
        public int maximumPlayers;
        public string gamemodeSettings;

        public SessionHandlerTradionalLobbyNGO.LobbyRepresentation.Room GenerateRoom()
        {
            SessionHandlerTradionalLobbyNGO.LobbyRepresentation.Room room = new SessionHandlerTradionalLobbyNGO.LobbyRepresentation.Room();
            room.roomTitle = title;
            room.selectedGamemode = gamemode;
            room.selectedMap = map;
            room.minimumPlayers = minimumPlayers;
            room.maximumPlayers = maximumPlayers;

            room.matchInProgressCode = string.Empty;
            room.players = new NativeList<int>(1, Allocator.Persistent);
            room.gamemodeSettings = gamemodeSettings;
            return room;
        }

        public bool IsValidRoom()
        {
            return !string.IsNullOrEmpty(gamemode) && !string.IsNullOrEmpty(map) &&
                   !string.IsNullOrEmpty(gamemodeSettings) && minimumPlayers > 1 &&
                   maximumPlayers >= minimumPlayers;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref title);
            serializer.SerializeValue(ref gamemode);
            serializer.SerializeValue(ref map);
            serializer.SerializeValue(ref minimumPlayers);
            serializer.SerializeValue(ref maximumPlayers);
            serializer.SerializeValue(ref gamemodeSettings);
        }

        public bool Equals(RoomUpdateInfo other)
        {
            return title == other.title && gamemode == other.gamemode && map == other.map &&
                   minimumPlayers == other.minimumPlayers && maximumPlayers == other.maximumPlayers &&
                   gamemodeSettings == other.gamemodeSettings;
        }

        public override bool Equals(object obj)
        {
            return obj is RoomUpdateInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(title, gamemode, map, minimumPlayers, maximumPlayers);
        }
    }
}