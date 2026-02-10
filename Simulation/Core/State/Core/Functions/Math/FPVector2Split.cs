using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class FPVector2Split : StateFunctionFP
    {
        public enum FPVector2ValueType
        {
            X,
            Y
        }

        public FPVector2ValueType valueWanted;
        public HNSFParamFPVector2 param;
    
        public override FP Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            var v2 = param.Resolve(frame, entity, ref stateContext);

            switch (valueWanted)
            {
                case FPVector2ValueType.X:
                    return v2.X;
                case FPVector2ValueType.Y:
                    return v2.Y;
            }
        
            return 0;
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new FPVector2Split());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as FPVector2Split;
            t.valueWanted = valueWanted;
            t.param = param.Clone() as HNSFParamFPVector2;
            return base.CopyTo(target);
        }
    }
}