using System;
using HnSF.core.state;
using HnSF.core.state.decisions;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF.StatusEffects.Components
{
    [System.Serializable]
    public unsafe partial class TargetHNSFStateDecisionsCheck : StatusEffectComponent
    {
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HNSFStateDecision[] conditions = Array.Empty<HNSFStateDecision>();

        public bool checkOnApply;
        public bool checkOnTick;
        public bool checkOnRemove;

        public override bool OnApply(Frame frame, EntityRef statusEffectEntityRef, StatusEffector* statusEffector)
        {
            if(checkOnApply == false)
                return true;

            return ValidateHnsfStateAgent(frame, statusEffector);
        }

        public override bool OnTick(Frame frame, EntityRef statusEffectEntityRef, StatusEffector* statusEffector)
        {
            if(checkOnTick == false)
                return true;
            
            return ValidateHnsfStateAgent(frame, statusEffector);
        }

        override public bool OnRemove(Frame frame, EntityRef statusEffectEntityRef, StatusEffector* statusEffector)
        {
            if(checkOnRemove == false)
                return true;
            
            return ValidateHnsfStateAgent(frame, statusEffector);
        }
        
        private bool ValidateHnsfStateAgent(Frame frame, StatusEffector* statusEffector)
        {
            var sc = new HNSFStateContext(frame, statusEffector->target);
            if (!CheckConditions(frame, statusEffector->target, ref sc)) return false;
            return true;
        }
        
        public bool CheckConditions(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (conditions == null || conditions.Length == 0) return true;
            foreach (var d in conditions)
            {
                if (d.Decide(frame, entity, ref stateContext) == false) return false;
            }
            return true;
        }
    }
}