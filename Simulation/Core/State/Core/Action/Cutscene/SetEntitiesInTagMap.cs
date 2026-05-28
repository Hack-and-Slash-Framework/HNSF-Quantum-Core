using Photon.Deterministic;
using System;
using HnSF.core.state.functions;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Cutscene/Set Entities In Tag Map")]
    public unsafe partial class SetEntitiesInTagMap : HNSFStateAction
    {
        [Serializable]
        public struct TagToEntityFunction
        {
            public AssetRef<Tag> entityTag;
            public StateFunctionEntityRef entityRefFunction;
        }
        
        public TagToEntityFunction[] tagToEntityRefFunctions = Array.Empty<TagToEntityFunction>();
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            var targetEntityRef = GetActionTargetEntityRef(frame, entity);
            if (targetEntityRef == EntityRef.None || !frame.Exists(targetEntityRef)) return false;

            frame.AddOrGet<TaggedEntityMapping>(targetEntityRef, out var tem);
            var mappingDict = frame.ResolveDictionary(tem->tagToEntityMap);
            
            foreach (var tagToEntityRefFunction in tagToEntityRefFunctions)
            {
                var entityRef = tagToEntityRefFunction.entityRefFunction.Execute(frame, targetEntityRef, ref stateContext);
                if (entityRef == EntityRef.None) continue;
                mappingDict.Add(tagToEntityRefFunction.entityTag, entityRef);
            }
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new SetEntitiesInTagMap());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as SetEntitiesInTagMap;
            t.tagToEntityRefFunctions = new TagToEntityFunction[tagToEntityRefFunctions.Length];
            for (int i = 0; i < tagToEntityRefFunctions.Length; i++)
            {
                t.tagToEntityRefFunctions[i].entityTag = tagToEntityRefFunctions[i].entityTag;
                t.tagToEntityRefFunctions[i].entityRefFunction = tagToEntityRefFunctions[i].entityRefFunction.Copy() as StateFunctionEntityRef;
            }
            return base.CopyTo(target);
        }
    }
}