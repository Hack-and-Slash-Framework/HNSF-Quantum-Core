using HnSF.core.state;
using Photon.Deterministic;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    public class StatePreviewQuantumConfiguration : AssetObject
    {
#if QUANTUM_UNITY
        [Header("Preview Actor")]
#endif
        public BattleActorDefinition testingCharacter;
        public FPVector2 resetPosition;
        public HNSFState chara1StartingState;
        public HNSFState[] chara1ValidStates;
        public FPVector2 startingVelocity;
        public bool flip;
        public ActorInputButtonType inputToSpam;
        public int spamEvery = 2;
        public ActorInputButtonType inputsToHold;
        
#if QUANTUM_UNITY
        [Header("Helping Actor")]
#endif
        public BattleActorDefinition testingCharacter2;
        public FPVector2 chara2StartPosition;
        public HNSFState chara2StartingState;
        public FPVector2 chara2StartingVelocity;
        public bool chara2Flip;

#if QUANTUM_UNITY
        [Header("Settings")]
#endif
        public FP globalDeltaMulti = 1;
        public FP localDeltaMulti = 1;

        public bool activePlayState = false;
        public bool lockStateChange;
    }
}