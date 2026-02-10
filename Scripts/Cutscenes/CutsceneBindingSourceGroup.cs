using System.Collections.Generic;
using Quantum;
using UnityEngine;

namespace HnSF
{
    [System.Serializable]
    public class CutsceneBindingSourceGroup : ITimelineDirectorBindingSource
    {
        public ITimelineDirectorBindingSource ParentSource => parent;
        public CutsceneBindingSource parent;

        public List<ITimelineDirectorBindingSource> sources = new();

        public Object GetMapping(AssetRef<Tag> tag)
        {
            for (int i = 0; i < sources.Count; i++)
            {
                if(sources[i].TryGetMapping(tag, out Object value)) return value;
            }
            return parent?.GetMapping(tag);
        }

        public bool TryGetMapping(AssetRef<Tag> tag, out Object output)
        {
            output = null;
            for (int i = 0; i < sources.Count; i++)
            {
                if (sources[i].TryGetMapping(tag, out output)) return true;
            }
            return false;
        }

        public bool TryGetMapping(AssetRef<Tag> tag, List<CutsceneBindingSourceCondition> conditions, out Object output)
        {
            output = null;
            for (int i = 0; i < sources.Count; i++)
            {
                if (sources[i].TryGetMapping(tag, out output)) return true;
            }
            return false;
        }
    }
}
