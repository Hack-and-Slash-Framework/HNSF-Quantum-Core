using Photon.Deterministic;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF.core.GroupControl.Combo
{
    public class ComboBattleScript : BattleActorGroupControlScript
    {
#if QUANTUM_UNITY
        [Header("Combo Info")]
#endif
        public FP baseWeight = 1;
        public FP idealRange;
        public int cooldownFrames;
        public int minimumRepeatGap;
    }
}
