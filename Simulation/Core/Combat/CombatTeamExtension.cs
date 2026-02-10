namespace Quantum
{
    public unsafe partial struct CombatTeam
    {
        public TeamBitmask GetHostilityMask(Frame frame)
        {
            if (!frame.TryFindAsset<GamemodeSettingsBase>(frame.RuntimeConfig.gamemodeConfigAsset, out var gamemodeConfigAsset)) return 0;
            return GetHostilityMask(gamemodeConfigAsset);
        }

        public TeamBitmask GetHostilityMask(GamemodeSettingsBase gamemodeSettings)
        {
            return gamemodeSettings.GetTeamConfig(value).hostilityMask;
        }

        public bool IsHostileTowards(Frame frame, CombatTeam* defenderTeam)
        {
            var selfHostilityMask = GetHostilityMask(frame);
            return selfHostilityMask.IsFlagSet(defenderTeam->value);
        }
        
        public bool IsHostileTowards(Frame frame, TeamBitmask defenderTeam)
        {
            var selfHostilityMask = GetHostilityMask(frame);
            return selfHostilityMask.IsFlagSet(defenderTeam);
        }
        
        public static bool IsHostileTowards(Frame frame, TeamBitmask hostilityMask, TeamBitmask defenderTeam)
        {
            return hostilityMask.IsFlagSet(defenderTeam);
        }
        
        public static TeamBitmask GetHostilityMask(Frame frame, EntityRef entityRef)
        {
            if(!frame.Unsafe.TryGetPointer<CombatTeam>(entityRef, out var cTeam)) return 0;
            return GetHostilityMask(frame, cTeam->value);
        }
        
        public static TeamBitmask GetHostilityMask(Frame frame, TeamBitmask team)
        {
            if (!frame.TryFindAsset<GamemodeSettingsBase>(frame.RuntimeConfig.gamemodeConfigAsset, out var gamemodeConfigAsset)) return 0;
            return GetHostilityMask(gamemodeConfigAsset, team);
        }
        
        public static TeamBitmask GetHostilityMask(GamemodeSettingsBase gamemodeSettings, TeamBitmask team)
        {
            return gamemodeSettings.GetTeamConfig(team).hostilityMask;
        }
    }
}
