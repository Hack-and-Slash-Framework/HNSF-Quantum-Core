using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class FPVector3Operation : StateFunctionFPVector3
    {
        public enum OperationType
        {
            ADD,
            SUBTRACT,
            MULTIPLY,
            DIVIDE
        }

        public OperationType operation;
        public HNSFParamFPVector3 valueA;
        public HNSFParamFPVector3 valueB;
        public bool normalize;
    
        public override FPVector3 Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            var vA = valueA.Resolve(frame, entity, ref stateContext);
            var vB = valueB.Resolve(frame, entity, ref stateContext);
            FPVector3 r;
        
            switch (operation)
            {
                case OperationType.ADD:
                    r = vA + vB;
                    if (normalize) r = r.Normalized;
                    return r;
                case OperationType.SUBTRACT:
                    r = vA - vB;
                    if (normalize) r = r.Normalized;
                    return r;
                case OperationType.MULTIPLY:
                    r = new FPVector3(vA.X * vB.X, vA.Y * vB.Y, vA.Z * vB.Z);
                    if (normalize) r = r.Normalized;
                    return r;
                case OperationType.DIVIDE:
                    r = new FPVector3(vA.X / vB.X, vA.Y / vB.Y, vA.Z / vB.Z);
                    if (normalize) r = r.Normalized;
                    return r;
            }
            return FPVector3.Zero;
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new FPVector3Operation());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as FPVector3Operation;
            t.operation = operation;
            t.valueA = valueA.Clone() as HNSFParamFPVector3;
            t.valueB = valueB.Clone() as HNSFParamFPVector3;
            t.normalize = normalize;
            return base.CopyTo(target);
        }
    }
}