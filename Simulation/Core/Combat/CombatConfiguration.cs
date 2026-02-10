using System.Collections.Generic;
using Photon.Deterministic;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    [System.Serializable]
    public partial class CombatConfiguration : AssetObject
    {
#if QUANTUM_UNITY
        [Header("Bounces")]
#endif
        public int maxWallBounces = 1;
        public int maxGroundBounces = 1;
        public int maxHardKnockdowns = 1;
        
#if QUANTUM_UNITY
        [Header("Combo Decay")]
#endif
        public FP globalComboDecay = FP.FromRaw(39321);
        public int comboDecayLaunchValueMinDecay = 15;
        public int comboDecayHorLaunchValueLimit = 60;
        public int comboDecayLaunchValueLimit = 90;
        public int comboDecayGravityMultiStartValue = 90;
        public int comboTimeBefore1Hitstun = 900;
        public FP sameMoveComboDecayMultiplier = FP._1_50;
        public FP sameMoveProrationMultiplier = FP._7;
        
        public int basePushStrength = 4;
    }
}
