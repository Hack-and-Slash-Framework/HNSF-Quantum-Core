using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class FPVector3Split : StateFunctionFP
    {
        public enum FPVector3ValueType
        {
            X,
            Y,
            Z,
            XPlusZ
        }

        public FPVector3ValueType valueWanted;
        public HNSFParamFPVector3 param;
    
        public override FP Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            var v3 = param.Resolve(frame, entity, ref stateContext);

            switch (valueWanted)
            {
                case FPVector3ValueType.X:
                    return v3.X;
                case FPVector3ValueType.Y:
                    return v3.Y;
                case FPVector3ValueType.Z:
                    return v3.Z;
                case FPVector3ValueType.XPlusZ:
                    return v3.X + v3.Z;
            }
        
            return 0;
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new FPVector3Split());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as FPVector3Split;
            t.valueWanted = valueWanted;
            t.param = param.Clone() as HNSFParamFPVector3;
            return base.CopyTo(target);
        }
    }
}