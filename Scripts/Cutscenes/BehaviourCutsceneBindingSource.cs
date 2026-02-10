using System;
using System.Collections.Generic;
using Quantum;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HnSF
{
    public class BehaviourCutsceneBindingSource : MonoBehaviour, ITimelineDirectorBindingSource
    {
        public ITimelineDirectorBindingSource ParentSource => parent;
        public CutsceneBindingSource parent;
        
        [System.Serializable]
        public class BindingData
        {
            public Tag tag;
            public GameObject GameObject;
        }
        
        public List<BindingData> bindings = new();
        
        [NonSerialized] public Dictionary<AssetRef<Tag>, Object> mappings = new();

        public void Initialize()
        {
            mappings.Clear();
            foreach(var b in bindings) mappings.Add(b.tag, b.GameObject);
        }
        
        public Object GetMapping(AssetRef<Tag> tag)
        {
            return mappings.TryGetValue(tag, out var mapping) ? mapping : parent?.GetMapping(tag);
        }

        public bool TryGetMapping(AssetRef<Tag> tag, out Object output)
        {
            return TryGetMapping(tag, null, out output);
        }

        public bool TryGetMapping(AssetRef<Tag> tag, List<CutsceneBindingSourceCondition> conditions, out Object output)
        {
            if (mappings.TryGetValue(tag, out output)) return true;
            return parent != null && parent.TryGetMapping(tag, out output);
        }
    }
}