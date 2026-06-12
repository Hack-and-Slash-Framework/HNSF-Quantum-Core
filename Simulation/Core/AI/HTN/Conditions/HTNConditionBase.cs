using System;
using Quantum;

namespace HnSF.core.AI.HTN.Conditions
{
    [Serializable]
    public unsafe partial class HTNConditionBase
    {
        public string Label;
        public bool disable;

        public virtual bool IsValid(Frame frame, EntityRef infoEntityRef)
        {
            return true;
        }
    }
}