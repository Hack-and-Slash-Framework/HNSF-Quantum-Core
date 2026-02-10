using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class GetGenericIntListCount : StateFunctionInt
    {
        public override int Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<GenericIntList>(entity, out var gil)) return 0;
            var intList = frame.ResolveList(gil->values);
            return intList.Count;
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new GetGenericIntListCount());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as GetGenericIntListCount;
            return base.CopyTo(target);
        }
    }
}