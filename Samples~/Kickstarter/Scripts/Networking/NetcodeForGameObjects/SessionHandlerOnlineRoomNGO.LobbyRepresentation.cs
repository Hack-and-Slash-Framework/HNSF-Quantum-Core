using System;
using Quantum;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace HnSF.sessionhandling.handlers.NGO
{
    [System.Serializable]
    public partial class SessionHandlerTradionalLobbyNGO : SessionHandlerTraditionalLobby
    {
        [System.Serializable]
        public partial class LobbyRepresentation
        {
            [System.Serializable]
            public struct Room : INetworkSerializable, IDisposable, System.IEquatable<Room>
            {
                public static readonly int InvalidRoomId = 0;

                public int roomId;
                public string roomTitle;
                public string matchInProgressCode;

                public string selectedGamemode;
                public string selectedMap;

                public NativeList<int> players;

                public int minimumPlayers;
                public int maximumPlayers;

                public string gamemodeSettings;

                /// <summary>
                /// Used to force dirty the room when changing the players list, since otherwise it won't dirty no matter
                /// what changes are made to players.
                /// </summary>
                public int garbage;

                public LobbyRoomStatus status;

                public double startMatchTime;
                
                public bool CanFitPlayerCount(int playerCount)
                {
                    var dt = DateTime.UtcNow.AddSeconds(10);
                    return (maximumPlayers - players.Length) >= playerCount;
                }

                public int GetRoomMasterPlayerId()
                {
                    if (players.Length == 0) return -1;
                    return players[0];
                }

                public bool MatchInProgress()
                {
                    return string.IsNullOrEmpty(matchInProgressCode) == false;
                }

                public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
                {
                    serializer.SerializeValue(ref roomId);
                    serializer.SerializeValue(ref roomTitle);
                    serializer.SerializeValue(ref matchInProgressCode);
                    serializer.SerializeValue(ref selectedGamemode);
                    serializer.SerializeValue(ref selectedMap);
                    serializer.SerializeValue(ref minimumPlayers);
                    serializer.SerializeValue(ref maximumPlayers);
                    serializer.SerializeValue(ref gamemodeSettings);
                    if(serializer.IsReader && players.IsCreated == false) players = new NativeList<int>(1, Allocator.Persistent);
                    serializer.SerializeValue(ref players);
                    serializer.SerializeValue(ref garbage);
                    serializer.SerializeValue(ref status);
                    serializer.SerializeValue(ref startMatchTime);
                }

                public void Dispose()
                {
                    if (players.IsCreated) players.Dispose();
                    players = default;
                }

                public bool Equals(Room other)
                {
                    return roomId == other.roomId
                           && roomTitle == other.roomTitle
                           && matchInProgressCode == other.matchInProgressCode &&
                           selectedGamemode == other.selectedGamemode && selectedMap == other.selectedMap &&
                           players.Length == other.players.Length && minimumPlayers == other.minimumPlayers &&
                           maximumPlayers == other.maximumPlayers && gamemodeSettings == other.gamemodeSettings
                           && other.garbage == garbage && other.status == status && Math.Abs(other.startMatchTime - startMatchTime) < 0.01;
                }

                public override bool Equals(object obj)
                {
                    return obj is Room other && Equals(other);
                }

                public override int GetHashCode()
                {
                    var hashCode = new HashCode();
                    hashCode.Add(roomId);
                    hashCode.Add(roomTitle);
                    hashCode.Add(matchInProgressCode);
                    hashCode.Add(selectedGamemode);
                    hashCode.Add(selectedMap);
                    hashCode.Add(players);
                    hashCode.Add(minimumPlayers);
                    hashCode.Add(maximumPlayers);
                    hashCode.Add(gamemodeSettings);
                    hashCode.Add(garbage);
                    hashCode.Add(status);
                    hashCode.Add(startMatchTime);
                    return hashCode.ToHashCode();
                }
            }

            [System.Serializable]
            public struct Player : INetworkSerializable, System.IEquatable<Player>
            {
                public static readonly int InvalidPlayerId = 0;

                public ulong clientId;
                public int clientLocalPlayerIndex;
                public int playerId;
                public string playerName;

                public int roomId;
                public bool ready;
                public MatchEndResult matchResult;

                public string[] selectedFighters;
                public int selectedTeam;
                
                public Player(ulong clientId, int clientLocalPlayerIndex, int playerId, string playerName, int roomId, TeamBitmask selectedTeam)
                {
                    this.clientId = clientId;
                    this.clientLocalPlayerIndex = clientLocalPlayerIndex;
                    this.playerId = playerId;
                    this.playerName = playerName;
                    this.roomId = roomId;
                    ready = false;
                    this.matchResult = MatchEndResult.None;
                    this.selectedFighters = new string[4];
                    this.selectedTeam = (int)selectedTeam;
                }

                public void ClearFighters()
                {
                    if (selectedFighters == null) return;
                    for (int i = 0; i < selectedFighters.Length; i++)
                    {
                        selectedFighters[i] = string.Empty;
                    }
                }

                public bool SetFighters(string[] fighters)
                {
                    if (fighters == null) return false;
                    ClearFighters();
                    
                    for (int i = 0; i < Mathf.Min(fighters.Length, selectedFighters.Length); i++)
                    {
                        selectedFighters[i] = fighters[i];
                    }
                    return true;
                }

                public bool SetTeam(TeamBitmask team)
                {
                    Debug.Log($"SetTeam: {team}");
                    this.selectedTeam = (int)team;
                    return true;
                }
                
                public void WriteField(ref FastBufferWriter writer)
                {
                    writer.WriteValueSafe(clientId);
                    writer.WriteValueSafe(clientLocalPlayerIndex);
                    writer.WriteValueSafe(playerId);
                    writer.WriteValueSafe(playerName);
                    writer.WriteValueSafe(roomId);
                    writer.WriteValueSafe(ready);
                    writer.WriteValueSafe(matchResult);
                    writer.WriteValueSafe(selectedTeam);
                    
                    writer.WriteValueSafe(selectedFighters.Length);

                    for (int i = 0; i < selectedFighters.Length; i++)
                    {
                        if (selectedFighters[i] == null) selectedFighters[i] = string.Empty;
                        writer.WriteValueSafe(selectedFighters[i]);
                    }
                }
                
                public void ReadField(ref FastBufferReader reader)
                {
                    reader.ReadValueSafe(out clientId);
                    reader.ReadValueSafe(out clientLocalPlayerIndex);
                    reader.ReadValueSafe(out playerId);
                    reader.ReadValueSafe(out playerName);
                    reader.ReadValueSafe(out roomId);
                    reader.ReadValueSafe(out ready);
                    reader.ReadValueSafe(out matchResult);
                    reader.ReadValueSafe(out selectedTeam);

                    var length = (int)0;
                    reader.ReadValueSafe(out length);
                    selectedFighters = new string[length];

                    for (int i = 0; i < length; i++)
                    {
                        var v = string.Empty;
                        reader.ReadValueSafe(out v);
                        selectedFighters[i] = v;
                    }
                }
                
                public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
                {
                    serializer.SerializeValue(ref clientId);
                    serializer.SerializeValue(ref clientLocalPlayerIndex);
                    serializer.SerializeValue(ref playerId);
                    serializer.SerializeValue(ref playerName);
                    serializer.SerializeValue(ref roomId);
                    serializer.SerializeValue(ref ready);
                    serializer.SerializeValue(ref matchResult);
                    serializer.SerializeValue(ref selectedTeam);

                    if (serializer.IsWriter)
                    {
                        int length = 0;
                        serializer.SerializeValue(ref length);
                        selectedFighters = new string[length];

                        for (int i = 0; i < length; i++)
                        {
                            string v = string.Empty;
                            serializer.SerializeValue(ref v);
                            selectedFighters[i] = v;
                        }
                    }
                    else
                    {
                        if (selectedFighters == null) selectedFighters = new string[4];
                        int length = selectedFighters.Length;
                        serializer.SerializeValue(ref length);

                        for (int i = 0; i < length; i++)
                        {
                            string v = selectedFighters[i];
                            serializer.SerializeValue(ref v);
                        }
                    }
                }
                
                public bool Equals(Player other)
                {
                    return clientId == other.clientId && clientLocalPlayerIndex == other.clientLocalPlayerIndex &&
                           playerId == other.playerId && playerName == other.playerName && roomId == other.roomId &&
                           ready == other.ready && matchResult == other.matchResult && selectedFighters.Equals(other.selectedFighters) &&
                           selectedTeam == other.selectedTeam;
                }

                public override bool Equals(object obj)
                {
                    return obj is Player other && Equals(other);
                }

                public override int GetHashCode()
                {
                    var hashCode = new HashCode();
                    hashCode.Add(clientId);
                    hashCode.Add(clientLocalPlayerIndex);
                    hashCode.Add(playerId);
                    hashCode.Add(playerName);
                    hashCode.Add(roomId);
                    hashCode.Add(ready);
                    hashCode.Add((int)matchResult);
                    hashCode.Add(selectedFighters);
                    hashCode.Add(selectedTeam);
                    return hashCode.ToHashCode();
                }
            }
        }
    }
}