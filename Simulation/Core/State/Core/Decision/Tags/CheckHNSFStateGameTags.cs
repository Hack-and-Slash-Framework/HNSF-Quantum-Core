using System;
using System.Linq;
using Quantum;
using Quantum.Addons.GameTags;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class CheckHNSFStateGameTags : HNSFStateDecision
    {
        public GameTagCollection mustHaveTags = new GameTagCollection();
        public GameTagCollection mustNotHaveTags = new GameTagCollection();
        
        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if(mustHaveTags.IsEmpty && mustNotHaveTags.IsEmpty)
                return true;
            
            var targetContext = new StateActionTargetContext(decisionTargetType, entity);
            var targetEntity = HNSFStateHelper.GetStateTargetEntity(frame, ref targetContext);
            if (targetEntity == EntityRef.None)
                return false;

            if(!frame.Unsafe.TryGetPointer<GenericStateMachine>(targetEntity, out var hnsfStateAgent))
                return false;
            if (!frame.TryFindAsset(hnsfStateAgent->stateAgent.stateData.state, out var currentStateAsset))
                return false;
            
            if (!currentStateAsset.gameTags.ContainsAll(frame, mustHaveTags))
                return false;
            return !currentStateAsset.gameTags.ContainsAny(frame, mustNotHaveTags);
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new CheckHNSFStateGameTags());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as CheckHNSFStateGameTags;
            t.mustHaveTags = new GameTagCollection
            {
                Tags = mustHaveTags?.Tags.ToList()
            };
            t.mustNotHaveTags = new GameTagCollection
            {
                Tags = mustNotHaveTags?.Tags.ToList()
            };
            return base.CopyTo(target);
        }
    }
}