using System;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class HasValidThroweeOfId : HNSFStateDecision
    {
        public int[] throweesToCheckByID;
        
        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<IsThrowing>(entity, out var isThrowing)) return false;
            var dict = frame.ResolveDictionary(isThrowing->throwees);

            for (int i = 0; i < throweesToCheckByID.Length; i++)
            {
                if (!dict.ContainsKey(throweesToCheckByID[i])) return false;
                if (!frame.Exists(dict[throweesToCheckByID[i]])) return false;
            }
            
            return true;
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new HasValidThroweeOfId());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as HasValidThroweeOfId;
            t.throweesToCheckByID = new int[throweesToCheckByID.Length];
            Array.Copy(throweesToCheckByID, t.throweesToCheckByID, throweesToCheckByID.Length);
            return base.CopyTo(target);
        }
    }
}