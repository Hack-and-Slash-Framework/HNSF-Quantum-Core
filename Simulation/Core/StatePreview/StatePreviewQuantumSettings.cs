using HnSF.core.state;
using Photon.Deterministic;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    public class StatePreviewQuantumSettings : GamemodeSettingsBase
    {
#if QUANTUM_UNITY
        [Header("Preview Actor")]
#endif
        public FPVector2 groundStartPosition;
        public FPVector2 aerialStartPosition;
        public FPVector2 startingVelocity;
        public bool flip;
        
#if QUANTUM_UNITY
        [Header("Helping Actor")]
#endif
        public BattleActorDefinition defenderActorDefinition;
        public FPVector2 defenderGroundStartPosition;
        public FPVector2 defenderAerialStartPosition;
        public FPVector2 chara2StartingVelocity;
        public bool defenderFlip = true;

#if QUANTUM_UNITY
        [Header("Settings")]
#endif
        public FP globalDeltaMulti = 1;
        public FP localDeltaMulti = 1;

        public bool activePlayState = false;
        public bool lockStateChange;
    }
}