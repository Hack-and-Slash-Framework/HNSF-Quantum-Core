using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class GetChargeLevel : StateFunctionInt
    {
        public override int Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (frame.Unsafe.TryGetPointer<IsChargingAttack>(entity, out var isa))
            {
                return isa->chargeLevel;
            }
            return 0;
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new GetChargeLevel());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as GetChargeLevel;
            return base.CopyTo(target);
        }
    }
}