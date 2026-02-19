using Quantum;

namespace HnSF.StatusEffects.Components
{
    [System.Serializable]
    public unsafe partial class StatusEffectComponent
    {
        public virtual bool OnApply(Frame frame)
        {
            return true;
        }

        public virtual bool OnTick(Frame frame)
        {
            return true;
        }

        public virtual bool OnRemove(Frame frame)
        {
            return true;
        }
    }
}
