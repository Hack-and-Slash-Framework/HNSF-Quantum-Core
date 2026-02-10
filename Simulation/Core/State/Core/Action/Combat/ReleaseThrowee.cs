using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class ReleaseThrowee : HNSFStateAction
    {
        public bool releaseAll;
        public int releasedThroweeID;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<IsThrowing>(entity, out var isThrowing)) return false;

            if (releaseAll)
            {
                isThrowing->ReleaseAllThrowees(frame, entity);
            }
            else
            {
                isThrowing->ReleaseThrowee(frame, entity, releasedThroweeID);
            }
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ReleaseThrowee());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ReleaseThrowee;
            t.releaseAll = releaseAll;
            t.releasedThroweeID = releasedThroweeID;
            return base.CopyTo(target);
        }
    }
}