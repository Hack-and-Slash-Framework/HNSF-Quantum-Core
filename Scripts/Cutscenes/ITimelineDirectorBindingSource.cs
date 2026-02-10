using System.Collections.Generic;
using Quantum;
using UnityEngine;

namespace HnSF
{
    public interface ITimelineDirectorBindingSource
    {
        public ITimelineDirectorBindingSource ParentSource { get; }
        public Object GetMapping(AssetRef<Tag> tag);
        public bool TryGetMapping(AssetRef<Tag> tag, out Object output);
        public bool TryGetMapping(AssetRef<Tag> tag, List<CutsceneBindingSourceCondition> conditions, out Object output);
    }
}