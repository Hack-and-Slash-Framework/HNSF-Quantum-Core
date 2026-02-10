using System.Collections.Generic;
using Quantum;
using UnityEngine;

namespace HnSF
{
    [System.Serializable]
    public class CutsceneBindingSource : ITimelineDirectorBindingSource
    {
        public ITimelineDirectorBindingSource ParentSource => parent;
        
        public CutsceneBindingSource parent;
        public Dictionary<AssetRef<Tag>, Object> mappings = new();

        public virtual Object GetMapping(AssetRef<Tag> tag)
        {
            return mappings.TryGetValue(tag, out var mapping) ? mapping : parent?.GetMapping(tag);
        }

        public virtual bool TryGetMapping(AssetRef<Tag> tag, out Object output)
        {
            return TryGetMapping(tag, null, out output);
        }
        
        public virtual bool TryGetMapping(AssetRef<Tag> tag, List<CutsceneBindingSourceCondition> conditions, out Object output)
        {
            if (mappings.TryGetValue(tag, out output)) return false;
            return parent != null && parent.TryGetMapping(tag, out output);
        }
    }
}