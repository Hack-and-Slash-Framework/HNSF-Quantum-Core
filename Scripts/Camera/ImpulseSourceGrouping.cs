using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace HnSF
{
    public class ImpulseSourceGrouping : MonoBehaviour
    {
        [Serializable]
        public class Grouping
        {
            public int shakeId;
            public CinemachineImpulseSource[] impulseSources = Array.Empty<CinemachineImpulseSource>();
        }

        [NonSerialized] public Dictionary<int, Grouping> idToImpulseGroup = new();
        public List<Grouping> groups = new();
        
        public void Initialize()
        {
            BuildGroupMap();
        }

        public void BuildGroupMap()
        {
            idToImpulseGroup.Clear();

            for (var index = 0; index < groups.Count; index++)
            {
                var group = groups[index];
                if (!idToImpulseGroup.TryAdd(group.shakeId, group))
                {
                    Debug.LogWarning($"Impulse group with id of {group.shakeId} already exists, skipping. (duplicate ID at index {index})");
                    continue;
                }
            }
        }
    }
}