using System;
using System.Collections.Generic;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    [System.Serializable]
    public class GamemodeSettingsBase : AssetObject
    {
        [NonSerialized] public Dictionary<TeamBitmask, GamemodeTeamConfig> teamToConfig = null;
        
        public GamemodeTeamRule[] teamRules = Array.Empty<GamemodeTeamRule>();
        public GamemodeTeamConfig[] teamConfigs = Array.Empty<GamemodeTeamConfig>();
        public int fightersPerPlayer = 1;
        public MatchParticipantInitialData[] initialParticipantsInfo = Array.Empty<MatchParticipantInitialData>();

        public virtual void Initialize()
        {
            BuildTeamToConfigMap();
        }
        
        protected virtual void BuildTeamToConfigMap()
        {
            teamToConfig = new Dictionary<TeamBitmask, GamemodeTeamConfig>();
            foreach (var teamConfig in teamConfigs) teamToConfig.Add(teamConfig.team, teamConfig);
        }
        
        public virtual GamemodeTeamConfig GetTeamConfig(TeamBitmask team)
        {
            return teamToConfig.GetValueOrDefault(team);
        }
        
        public virtual GamemodeSettingsBase CreateSettingsAsset()
        {
            var asset = AssetObject.Create<GamemodeSettingsBase>();
            FillBaseSettings(asset);
            return asset;
        }

        public virtual void FillBaseSettings(GamemodeSettingsBase asset)
        {
            asset.teamRules = teamRules;
            asset.teamConfigs = teamConfigs;
            asset.fightersPerPlayer = fightersPerPlayer;
            asset.initialParticipantsInfo = initialParticipantsInfo;
        }

        public virtual GamemodeSettingsBase GetInstance()
        {
#if QUANTUM_UNITY
            var instance = ScriptableObject.CreateInstance<GamemodeSettingsBase>();
            instance.FillBaseSettings(this);
            return instance;
#else
            return null;
#endif
        }
        
        public virtual ConfigurableSettingBase[] GetConfigurableSettings()
        {
            var settings = Array.Empty<ConfigurableSettingBase>();
            return settings;
        }

        public virtual void SetConfigurableSettings(ConfigurableSettingBase[] settings)
        {
            
        }
    }
}