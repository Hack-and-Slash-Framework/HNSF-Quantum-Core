using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Photon.Deterministic;
using Quantum;
using UnityEngine.Scripting;

namespace HnSF.core.systems
{
    [Preserve]
    public unsafe partial class GenericGamemodeStateSystem : SystemMainThread, ISignalOnComponentAdded<GenericGamemodeGlobals>, ISignalOnComponentRemoved<GenericGamemodeGlobals>
    {
        static readonly ReadOnlyDictionary<GenericGamemodeStates, Type> stateTable =
            new(new Dictionary<GenericGamemodeStates, Type>()
            {
                { GenericGamemodeStates.Lobby, typeof(IGameStateGroupLobby) },
                { GenericGamemodeStates.Pregame, typeof(IGameStateGroupPregame) },
                { GenericGamemodeStates.MapIntro, typeof(IGameStateGroupMapIntro) },
                { GenericGamemodeStates.CharacterIntro, typeof(IGameStateGroupCharacterIntro) },
                { GenericGamemodeStates.Countdown, typeof(IGameStateGroupCountdown) },
                { GenericGamemodeStates.Game, typeof(IGameStateGroupGame) },
                { GenericGamemodeStates.Outro, typeof(IGameStateGroupOutro) },
                { GenericGamemodeStates.VictoryScreen, typeof(IGameStateGroupVictory) },
                { GenericGamemodeStates.Postgame, typeof(IGameStateGroupPostgame) }
            });

        public override void OnInit(Frame f)
        {
            var gamemodeGlobals = f.Unsafe.GetOrAddSingletonPointer<GenericGamemodeGlobals>();

            gamemodeGlobals->DelayedState = 0;
            gamemodeGlobals->DelayedStateTimer = 0;
            gamemodeGlobals->PreviousState = 0;
            SetState(f, GenericGamemodeStates.Pregame);
        }

        public override void Update(Frame f)
        {
            var gamemodeGlobals = f.Unsafe.GetOrAddSingletonPointer<GenericGamemodeGlobals>();
            
            if (gamemodeGlobals->DelayedStateTimer > 0)
            {
                gamemodeGlobals->DelayedStateTimer -= f.DeltaTime;
                if (gamemodeGlobals->DelayedStateTimer <= 0)
                {
                    gamemodeGlobals->DelayedStateTimer = 0;
                    SetState(f, gamemodeGlobals->DelayedState);
                    gamemodeGlobals->DelayedState = 0;
                }
            }

            if (gamemodeGlobals->CurrentState != gamemodeGlobals->PreviousState)
            {
                Log.Debug($"Updating Gamemode State from {gamemodeGlobals->PreviousState} to {gamemodeGlobals->CurrentState}");
                if (stateTable.TryGetValue(gamemodeGlobals->CurrentState, out Type t))
                {
                    foreach (SystemBase sys in f.SystemsAll)
                    {
                        Type syst = sys.GetType();
                        bool syst_anystate = syst.GetInterfaces().Contains(typeof(IGameStateGroup));
                        if (syst_anystate)
                        {
                            Type _t = t;
                            bool syst_thisstate = syst.GetInterfaces().Contains(_t);
                        }
                    }

                    foreach (SystemBase sys in f.SystemsAll
                                 .Where(s => s.GetType().GetInterfaces().Contains(typeof(IGameStateGroup)) && !s.GetType().GetInterfaces().Contains(t)))
                    {
                        f.SystemDisable(sys.GetType());
                    }

                    foreach (SystemBase sys in f.SystemsAll
                                 .Where(s => s.GetType().GetInterfaces().Contains(typeof(IGameStateGroup)) && s.GetType().GetInterfaces().Contains(t)))
                    {
                        if (!f.SystemAnyEnabledSelf(sys.GetType()))
                            f.SystemEnable(sys.GetType());
                    }
                }
                
                f.Signals.GamemodeStateChanged(gamemodeGlobals->CurrentState, gamemodeGlobals->PreviousState);
                f.Events.GamemodeStateChanged(gamemodeGlobals->CurrentState, gamemodeGlobals->PreviousState);
                gamemodeGlobals->PreviousState = gamemodeGlobals->CurrentState;
            }
        }
        
        public static void SetStateDelayed(Frame f, GenericGamemodeStates state, FP delay)
        {
            var gamemodeGlobals = f.Unsafe.GetOrAddSingletonPointer<GenericGamemodeGlobals>();
            
            gamemodeGlobals->DelayedState = state;
            gamemodeGlobals->DelayedStateTimer = delay;
        }

        public static void SetState(Frame f, GenericGamemodeStates state)
        {
            var gamemodeGlobals = f.Unsafe.GetOrAddSingletonPointer<GenericGamemodeGlobals>();
            
            gamemodeGlobals->CurrentState = state;
        }

        public void OnAdded(Frame f, EntityRef entity, GenericGamemodeGlobals* component)
        {
        }

        public void OnRemoved(Frame f, EntityRef entity, GenericGamemodeGlobals* component)
        {
        }
    }
}