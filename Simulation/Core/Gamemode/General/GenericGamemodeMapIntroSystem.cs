using Photon.Deterministic;
using Quantum;
using UnityEngine.Scripting;

namespace HnSF.core.systems
{
    [Preserve]
    public unsafe partial class MapIntroSystem : SystemMainThread, IGameStateGroupMapIntro
    {
        public override bool StartEnabled => false;

        public override void OnEnabled(Frame f)
        {
            base.OnEnabled(f);
            Log.Debug("Gamemode Intro: Awaiting Map Intro Finished.");
        }

        public override void Update(Frame f)
        {
            var gamemodeGlobals = f.Unsafe.GetOrAddSingletonPointer<GenericGamemodeGlobals>();

            var playersFinishedIntro = f.ResolveHashSet(gamemodeGlobals->playersFinishedIntro);
            
            for (int i = 0; i < f.MaxPlayerCount; i++)
            {
                var isPlayerConnected = (f.GetPlayerInputFlags(i) & DeterministicInputFlags.PlayerNotPresent) == 0;
                
                if (isPlayerConnected == false || f.GetPlayerCommand(i) is IntroFinishedCommand)
                    playersFinishedIntro.Add(i);
            }

            if (playersFinishedIntro.Count < f.MaxPlayerCount) return;
            playersFinishedIntro.Clear();
            GenericGamemodeStateSystem.SetState(f, GenericGamemodeStates.CharacterIntro);
        }
    }
}