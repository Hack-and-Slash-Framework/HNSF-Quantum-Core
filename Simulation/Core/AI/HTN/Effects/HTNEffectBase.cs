using System;
using Quantum;

namespace HnSF.core.AI.HTN.Effects
{
    [Serializable]
    public unsafe partial class HTNEffectBase
    {
        public string Label;
        public bool disable;

        public virtual void Apply(Frame frame, EntityRef infoEntityRef)
        {
            
        }
    }
}