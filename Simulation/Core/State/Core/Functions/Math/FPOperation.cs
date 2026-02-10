using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class FPOperation : StateFunctionFP
    {
        public enum OperationType
        {
            MULTIPLY,
            DIVIDE,
            ADD,
            SUBTRACT
        }

        public OperationType operation;
        public HNSFParamFP valueA;
        public HNSFParamFP valueB;
    
        public override FP Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            var vA = valueA.Resolve(frame, entity, ref stateContext);
            var vB = valueB.Resolve(frame, entity, ref stateContext);
        
            switch (operation)
            {
                case OperationType.MULTIPLY:
                    return vA * vB;
                case OperationType.DIVIDE:
                    return vA / vB;
                case OperationType.ADD:
                    return vA + vB;
                case OperationType.SUBTRACT:
                    return vA - vB;
            }
            return 0;
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new FPOperation());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as FPOperation;
            t.operation = operation;
            t.valueA = valueA.Clone() as HNSFParamFP;
            t.valueB = valueB.Clone() as HNSFParamFP;
            return base.CopyTo(target);
        }
    }
}