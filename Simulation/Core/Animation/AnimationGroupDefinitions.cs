using System;
using System.Collections.Generic;
using Photon.Deterministic;
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
#endif

namespace Quantum
{
    public partial class AnimationGroupDefinitions : AssetObject
    {
        public AssetRef<AnimationGroupDefinitions> template;
        [NonSerialized] private Dictionary<AssetRef<Tag>, AssetRef<AnimationEntry>> tagToAnimation = null;
        public List<AssetRef<AnimationEntry>> animationEntries = new List<AssetRef<AnimationEntry>>();

#if UNITY_EDITOR
        [EditorButton("Create Animation Entry")]
        public void CreateAndAddAnimationEntry()
        {
            var saveFolder = AssetDatabase.GetAssetPath(this);
            var saveLocation = EditorUtility.SaveFilePanelInProject("Save State Asset", $"New AnimationEntry", 
                "asset", "Please give the location to save the state.", System.IO.Path.GetDirectoryName(saveFolder));
            if (string.IsNullOrEmpty(saveLocation)) return;
            
            var assPath = AssetDatabase.GenerateUniqueAssetPath(saveLocation);
            var asset = ScriptableObject.CreateInstance<AnimationEntry>();
            AssetDatabase.CreateAsset(asset, assPath);
            AssetDatabase.SaveAssetIfDirty(AssetDatabase.GUIDFromAssetPath(assPath));
            AssetDatabase.ImportAsset(assPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();

            var realStateAsset = AssetDatabase.LoadMainAssetAtPath(assPath) as AnimationEntry;
            
            animationEntries.Add(realStateAsset);
            EditorUtility.SetDirty(this);
        }

        [EditorButton("Refresh")]
        public void Refresh()
        {
            tagToAnimation.Clear();
        }

        [EditorButton("Validate")]
        public void ValidateAnimationEntries()
        {
            
        }
        
        [EditorButton("Update")]
        public void UpdateAnimationEntries()
        {
            tagToAnimation = null;
            animationEntries.Clear();
            var myPath = AssetDatabase.GetAssetPath(this);
            myPath = System.IO.Path.GetDirectoryName(myPath);
            var animationEntryGuids = AssetDatabase.FindAssets($"t:{nameof(AnimationEntry)}", new string[] { myPath });
            
            Undo.RecordObject(this, "Updated Animation Entries");
            foreach (var guid in animationEntryGuids)
            {
                var asset = AssetDatabase.LoadAssetByGUID<AnimationEntry>(new GUID(guid));
                if (asset == null) continue;
                animationEntries.Add(asset);
            }
            EditorUtility.SetDirty(this);
        }
#endif

        public override void Loaded(IResourceManager resourceManager, Native.Allocator allocator)
        {
            base.Loaded(resourceManager, allocator);
            tagToAnimation = null;
        }

        public AssetRef<AnimationEntry> GetAnimationByTag(Frame frame, AssetRef<Tag> tag)
        {
            if(tagToAnimation is null) BuildDictionary(frame);
            return tagToAnimation.TryGetValue(tag, out AssetRef<AnimationEntry> animation) ? animation : null;
        }

        public bool TryGetAnimationByTag(Frame frame, AssetRef<Tag> tag, out AssetRef<AnimationEntry> animation)
        {
            if(tagToAnimation is null) BuildDictionary(frame);
            return tagToAnimation.TryGetValue(tag, out animation);
        }

        private void BuildDictionary(Frame frame)
        {
            tagToAnimation = new Dictionary<AssetRef<Tag>, AssetRef<AnimationEntry>>();
            foreach (var ae in animationEntries)
            {
                if (frame.TryFindAsset(ae, out var animationEntryAsset) == false || animationEntryAsset.sharedAnimationTag == default) continue;
                tagToAnimation.TryAdd(animationEntryAsset.sharedAnimationTag, animationEntryAsset);
            }
            
            BuildDictionaryWithTemplate(frame, template);
        }

        private void BuildDictionaryWithTemplate(Frame frame, AssetRef<AnimationGroupDefinitions> assetRef)
        {
            if (!frame.TryFindAsset(assetRef, out var templateAsset)) return;
            
            foreach (var ae in templateAsset.animationEntries)
            {
                if (frame.TryFindAsset(ae, out var animationEntryAsset) == false || animationEntryAsset.sharedAnimationTag == default) continue;
                tagToAnimation.TryAdd(animationEntryAsset.sharedAnimationTag, animationEntryAsset);
            }
            
            BuildDictionaryWithTemplate(frame, templateAsset.template);
        }
    }
}