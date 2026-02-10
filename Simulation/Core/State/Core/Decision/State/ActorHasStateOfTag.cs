using System;
using System.Linq;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class ActorHasStateOfTag : HNSFStateDecision
    {
        public AssetRef<Tag>[] validStates;
    
        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            switch (decisionTargetType)
            {
                case StateActionTargetType.Self:
                    return DoDecision(frame, entity);
            }
            return false;
        }

        private bool DoDecision(Frame frame, EntityRef entity)
        {
            if (!frame.Unsafe.TryGetPointer<GenericStateMachine>(entity, out var csm)
                || !frame.TryFindAsset(csm->stateAgent.stateSet, out var stateSet)) return false;
            
            foreach (var vs in validStates)
            {
                if (stateSet.HasStateWithTag(csm->stateAgent.stateData.moveset, vs)) return true;
            }
            return false;
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new ActorHasStateOfTag());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as ActorHasStateOfTag;
            t.validStates = validStates.ToArray();
            return base.CopyTo(target);
        }
    }
}