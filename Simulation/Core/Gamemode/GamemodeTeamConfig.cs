using Quantum;

[System.Serializable]
public struct GamemodeTeamConfig
{
    public string name;
    public TeamBitmask team;
    public TeamBitmask hostilityMask;
    public ColorRGBA color;
    public int teamRuleId;
    public bool canBeDisabled;
}