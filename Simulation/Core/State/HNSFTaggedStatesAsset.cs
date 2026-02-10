using System;
using System.Collections.Generic;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF.core.state
{
    [Serializable]
    public class HNSFTaggedStatesAsset : AssetObject
    {
        [System.Serializable]
        public class TagHNSFStateDefinition
        {
            public string name;
            public AssetRef<Tag> tag;
            public AssetRef<HNSFState> state;
        }

        [NonSerialized] private Dictionary<AssetRef<Tag>, AssetRef<HNSFState>> tagToState = null;
        [NonSerialized] private Dictionary<AssetRef<HNSFState>, AssetRef<Tag>> stateToTag = null;
#if QUANTUM_UNITY
        [SerializeField] 
#endif
        private TagHNSFStateDefinition[] tagStateMap;

        public AssetRef<HNSFState> Get(AssetRef<Tag> tagRef)
        {
            if (tagToState == null || tagToState.Count != tagStateMap.Length) BuildTagStateMap();
            return tagToState.TryGetValue(tagRef, out var v) ? v : null;
        }

        public bool TryGetValue(AssetRef<Tag> tagRef, out AssetRef<HNSFState> stateRef)
        {
            if (tagToState == null  || tagToState.Count != tagStateMap.Length) BuildTagStateMap();
            return tagToState.TryGetValue(tagRef, out stateRef);
        }
        
        public bool TryGetValue(AssetRef<HNSFState> stateRef, out AssetRef<Tag> tagRef)
        {
            if (stateToTag == null || stateToTag.Count != tagStateMap.Length) BuildTagStateMap();
            return stateToTag.TryGetValue(stateRef, out tagRef);
        }

        private void BuildTagStateMap()
        {
            tagToState = new();
            stateToTag = new();
            for (var index = 0; index < tagStateMap.Length; index++)
            {
                var ts = tagStateMap[index];
                if (
                    (ts.tag == null || tagToState.TryAdd(ts.tag, ts.state))
                    && (ts.state == null || stateToTag.TryAdd(ts.state, ts.tag))
                    ) continue;
                Log.DebugError($"{name} already registered tag (index {index}).");
            }
        }
    }
}