namespace HnSF.sessionhandling.handlers
{
    [System.Serializable]
    public enum LobbyRoomStatus
    {
        WaitingForPlayers,
        CountingDown,
        AwaitingMatchCode,
        MatchInProgress
    }
}