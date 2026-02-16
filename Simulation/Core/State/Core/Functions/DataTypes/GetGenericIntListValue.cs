using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class GetGenericIntListValue : StateFunctionInt
    {
        public enum StartingDirectionType
        {
            FromStart,
            FromEnd
        }

        public StartingDirectionType startingDirection = StartingDirectionType.FromEnd;
        public int offset;
        
        public override int Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<GenericIntList>(entity, out var gil)) return 0;
            var intList = frame.ResolveList(gil->values);
            if (intList.Count <= offset) return 0;
            return startingDirection == StartingDirectionType.FromStart ? intList[offset] : intList[intList.Count - 1 - offset];
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new GetGenericIntListValue());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as GetGenericIntListValue;
            t.startingDirection = startingDirection;
            t.offset = offset;
            return base.CopyTo(target);
        }
    }
}