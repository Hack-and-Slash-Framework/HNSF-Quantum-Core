using UnityEngine;

namespace Quantum
{
    public unsafe partial struct TaggedEntityMapping
    {
        public static EntityRef GetEntityFromMap(Frame frame, EntityRef callingEntity, AssetRef<Tag> tag)
        {
            if(!frame.Unsafe.TryGetPointer<TaggedEntityMapping>(callingEntity, out var taggedEntityMapping)) return EntityRef.None;
            var mappingDict = frame.ResolveDictionary(taggedEntityMapping->tagToEntityMap);
            return mappingDict.TryGetValue(tag, out var mapEntityRef) ? mapEntityRef : EntityRef.None;
        }
    }
}
