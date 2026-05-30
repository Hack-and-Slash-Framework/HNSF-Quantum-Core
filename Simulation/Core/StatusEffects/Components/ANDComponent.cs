using System;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF.StatusEffects.Components
{
    [System.Serializable]
    public unsafe partial class ANDComponent : StatusEffectComponent
    {
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public StatusEffectComponent[] components = Array.Empty<StatusEffectComponent>();
        
        public override bool OnApply(Frame frame, EntityRef statusEffectEntityRef, StatusEffector* statusEffector)
        {
            foreach (var component in components)
            {
                if (!component.OnApply(frame, statusEffectEntityRef, statusEffector))
                    return false;
            }
            return true;
        }
        
        public override bool OnTick(Frame frame, EntityRef statusEffectEntityRef, StatusEffector* statusEffector)
        {
            foreach (var component in components)
            {
                if (!component.OnTick(frame, statusEffectEntityRef, statusEffector))
                    return false;
            }
            return true;
        }
        public override bool OnRemove(Frame frame, EntityRef statusEffectEntityRef, StatusEffector* statusEffector)
        {
            foreach (var component in components)
            {
                component.OnRemove(frame, statusEffectEntityRef, statusEffector);
            }
            return true;
        }
    }
}