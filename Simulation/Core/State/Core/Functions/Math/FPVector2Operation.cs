using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class FPVector2Operation : StateFunctionFPVector2
    {
        public enum OperationType
        {
            ADD,
            SUBTRACT,
            MULTIPLY,
            DIVIDE
        }

        public OperationType operation;
        public HNSFParamFPVector2 valueA;
        public HNSFParamFPVector2 valueB;
        public bool normalize;
    
        public override FPVector2 Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            var vA = valueA.Resolve(frame, entity, ref stateContext);
            var vB = valueB.Resolve(frame, entity, ref stateContext);
            FPVector2 r;
        
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
                    r = new FPVector2(vA.X * vB.X, vA.Y * vB.Y);
                    if (normalize) r = r.Normalized;
                    return r;
                case OperationType.DIVIDE:
                    r = new FPVector2(vA.X / vB.X, vA.Y / vB.Y);
                    if (normalize) r = r.Normalized;
                    return r;
            }
            return FPVector2.Zero;
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new FPVector2Operation());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as FPVector2Operation;
            t.operation = operation;
            t.valueA = valueA.Clone() as HNSFParamFPVector2;
            t.valueB = valueB.Clone() as HNSFParamFPVector2;
            t.normalize = normalize;
            return base.CopyTo(target);
        }
    }
}