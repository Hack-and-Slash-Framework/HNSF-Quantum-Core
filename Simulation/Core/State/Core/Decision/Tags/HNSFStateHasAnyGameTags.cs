using System;
using System.Linq;
using Quantum;
using Quantum.Addons.GameTags;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class HNSFStateHasAnyGameTags : HNSFStateDecision
    {
        public GameTagCollection tags = new GameTagCollection();
        
        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if(tags.IsEmpty)
                return true;
            
            var targetContext = new StateActionTargetContext(decisionTargetType, entity);
            var targetEntity = HNSFStateHelper.GetStateTargetEntity(frame, ref targetContext);
            if (targetEntity == EntityRef.None)
                return false;
            
            if(!frame.Unsafe.TryGetPointer<GenericStateMachine>(targetEntity, out var hnsfStateAgent))
                return false;
            if (!frame.TryFindAsset(hnsfStateAgent->stateAgent.stateData.state, out var currentStateAsset))
                return false;
            
            return currentStateAsset.allGameTags.ContainsAny(frame, tags);
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new HNSFStateHasAnyGameTags());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as HNSFStateHasAnyGameTags;
            t.tags = new GameTagCollection
            {
                Tags = tags.Tags.ToList()
            };
            return base.CopyTo(target);
        }
    }
}