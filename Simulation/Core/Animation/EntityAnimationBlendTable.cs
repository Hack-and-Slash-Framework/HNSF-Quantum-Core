using System;
using System.Collections.Generic;
using Photon.Deterministic;
using Quantum;
using UnityEngine;

namespace HnSF
{
    [Serializable]
    public class EntityAnimationBlendTable
    {
        public Dictionary<AssetRef<AnimationEntry>, float> blends = new();
        
        [Serializable]
        public struct Entry
        {
            public AssetRef<AnimationEntry> other;
            public float fadeToTime;
        }
        
        public Entry[] entries = Array.Empty<Entry>();

        public void BuildDictionary()
        {
            blends.Clear();
            foreach (var entry in entries) blends.Add(entry.other, entry.fadeToTime);
        }
    }
}