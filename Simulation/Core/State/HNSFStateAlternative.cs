using System.Collections.Generic;
using HnSF.core.state;
using HnSF.core.state.actions;
using Photon.Deterministic;

namespace Quantum
{
    public class HNSFStateAlternative : HNSFState
    {
        public AssetRef<HNSFState> referenceState;

        public override unsafe bool Execute(ref FrameThreadSafe frame, FP deltaTime, HNSFStateAgentData* hnsfData, EntityRef entity,
            ref HNSFStateContext context)
        {
            if (!frame.TryFindAsset(referenceState, out var refState))
                return false;
            return refState.Execute(ref frame, deltaTime, hnsfData, entity, ref context);
        }

        public override unsafe bool Execute(ref FrameThreadSafe frame, FP deltaTime, HNSFStateAgentData* hnsfData, EntityRef entity,
            ref HNSFStateContext context, Dictionary<AssetRef<HNSFState>, HashSet<int>> ignored)
        {
            if (!frame.TryFindAsset(referenceState, out var refState))
                return false;
            return refState.Execute(ref frame, deltaTime, hnsfData, entity, ref context, ignored);
        }

        public override bool CheckConditions(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.TryFindAsset(referenceState, out var refState))
                return false;
            return refState.CheckConditions(frame, entity, ref stateContext);
        }

        public override void CopyTo(HNSFState target)
        {
            (target as HNSFStateAlternative).referenceState = referenceState;
            base.CopyTo(target);
        }
    }
}
