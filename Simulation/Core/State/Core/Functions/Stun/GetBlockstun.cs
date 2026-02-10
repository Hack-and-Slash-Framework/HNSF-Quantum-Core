using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class GetBlockstun : StateFunctionInt
    {
        public override int Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<Blockstun>(entity, out var blockstun)) return 0;
            return blockstun->value;
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new GetBlockstun());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            return base.CopyTo(target);
        }
    }
}