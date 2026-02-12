using Cysharp.Threading.Tasks;
using Quantum;
using UnityEngine;

[System.Serializable]
public abstract partial class BaseGamemodeDefinition : IContentDefinition
{
    public abstract int MinimumPlayers { get; protected set; }
    public abstract int MaximumPlayers { get; protected set; }
    public abstract GamemodeTeamRule[] GetTeamRules();
    public abstract GamemodeTeamConfig[] GetDefaultTeamConfig();
    public abstract GamemodeSettingsBase GetDefaultGamemodeSettings();
    public abstract GamemodeSettingsBase GetGamemodeSettingsInstance();
    public abstract GameObject GetMatchHandler();
}