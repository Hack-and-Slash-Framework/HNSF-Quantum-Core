using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class GetChargePartition : StateFunctionInt
    {
        public override int Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (frame.Unsafe.TryGetPointer<IsChargingAttack>(entity, out var isa))
            {
                return isa->currentCharge;
            }
            return 0;
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new GetChargePartition());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as GetChargePartition;
            return base.CopyTo(target);
        }
    }
}