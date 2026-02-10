using System;
using System.Linq;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class CurrentMovesetIs : HNSFStateDecision
    {
        public AssetRef<Tag>[] wantedMoveset = Array.Empty<AssetRef<Tag>>();
        
        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            return Array.IndexOf(wantedMoveset, stateContext.agentData->moveset) >= 0;
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new CurrentMovesetIs());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as CurrentMovesetIs;
            t.wantedMoveset = wantedMoveset.ToArray();
            return base.CopyTo(target);
        }
    }
}