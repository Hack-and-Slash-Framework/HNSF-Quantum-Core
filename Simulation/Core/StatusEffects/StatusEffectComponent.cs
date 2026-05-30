using Quantum;

namespace HnSF.StatusEffects.Components
{
    [System.Serializable]
    public unsafe partial class StatusEffectComponent
    {
#if QUANTUM_UNITY
        public virtual void OnValidate(AssetObject statusEffectAsset)
        {
            
        }
#endif
        
        public virtual bool OnApply(Frame frame, EntityRef statusEffectEntityRef, StatusEffector* statusEffector)
        {
            return true;
        }

        public virtual bool OnTick(Frame frame, EntityRef statusEffectEntityRef, StatusEffector* statusEffector)
        {
            return true;
        }

        public virtual bool OnRemove(Frame frame, EntityRef statusEffectEntityRef, StatusEffector* statusEffector)
        {
            return true;
        }
    }
}
