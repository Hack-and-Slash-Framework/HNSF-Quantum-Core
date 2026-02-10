using System;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class AttemptAssignTaggedEntity : HNSFStateDecision
    {
        public AssetRef<Tag> tagRef;
        public HNSFParamEntityRef entityRefParam;
        public bool clearTaggedEntities;
        
        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            frame.AddOrGet<TaggedEntityMapping>(entity, out var tem);
            var mappingDict = frame.ResolveDictionary(tem->tagToEntityMap);
            
            if (clearTaggedEntities)
            {
                mappingDict.Clear();
            }
            
            var entityRef = entityRefParam.Resolve(frame, entity, ref stateContext);
            if (entityRef == EntityRef.None || !frame.Exists(entityRef)) return false;
            mappingDict.TryAdd(tagRef, entityRef);
            return true;
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new AttemptAssignTaggedEntity());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as AttemptAssignTaggedEntity;
            return base.CopyTo(target);
        }
    }
}