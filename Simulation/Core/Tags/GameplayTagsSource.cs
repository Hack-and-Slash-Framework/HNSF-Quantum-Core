using System;
using System.Collections.Generic;
using Photon.Deterministic;
using UnityEngine;
using UnityEngine.Events;

namespace Quantum
{
    public unsafe partial class GameplayTagsSource : AssetObject
    {
        [NonSerialized] public Dictionary<ushort, AssetRef<Tag>> shortIdToAssetRef = new();
        [NonSerialized] public Dictionary<AssetRef<Tag>, ushort> tagAssetRefToShortId = new();
        
        public string newTagLocation;
        public List<AssetRef<Tag>> GameplayTags = new List<AssetRef<Tag>>();
        
        [NonSerialized] public UnityEvent OnTagCreated = new UnityEvent();

        public override void Loaded(IResourceManager resourceManager, Native.Allocator allocator)
        {
            base.Loaded(resourceManager, allocator);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying) return;
            for (int i = GameplayTags.Count - 1; i >= 0; i--)
            {
                if(GameplayTags[i] == null) GameplayTags.RemoveAt(i);
            }
        }
#endif

        public List<Tag> GetBaseTags(Frame frame)
        {
            List<Tag> baseTags = new List<Tag>();
            for (int i = 0; i < GameplayTags.Count; i++)
            {
                if(GameplayTags[i] == default) continue;
                var tag = frame.FindAsset(GameplayTags[i]);
                if (tag == null || tag.parent != null) continue;
                baseTags.Add(tag);
            }

            return baseTags;
        }
        
        public static void FindBranchRecursive(Frame frame, Tag currTag, string[] tagParts, ref Tag branchAsset, ref int matchedParts)
        {
            if(matchedParts >= tagParts.Length) return;

            for (int i = 0; i < currTag.childTags.Count; i++)
            {
                var childTag = frame.FindAsset<Tag>(currTag.childTags[i]);
                if(childTag.label != tagParts[matchedParts]) continue;
                matchedParts++;
                branchAsset = childTag;
                FindBranchRecursive(frame, childTag, tagParts, ref branchAsset, ref matchedParts);
                break;
            }
        }
    }
}