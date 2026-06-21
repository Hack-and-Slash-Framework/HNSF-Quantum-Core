using System;
using System.Collections.Generic;
using Photon.Deterministic;
#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif

namespace Quantum
{
    public partial class Soundbank : AssetObject
    {
        public string label;
        public AssetRef<Tag> tag;
        public List<AssetRef<SoundEntry>> sounds = new List<AssetRef<SoundEntry>>();
        [NonSerialized] private Dictionary<AssetRef<Tag>, AssetRef<SoundEntry>> tagToSoundEntry = null;

        public override void Loaded(IResourceManager resourceManager, Native.Allocator allocator)
        {
            base.Loaded(resourceManager, allocator);
            
            tagToSoundEntry = new Dictionary<AssetRef<Tag>, AssetRef<SoundEntry>>();
            foreach (var ae in sounds)
            {
                if (resourceManager.TryGetAsset(ae, out var soundEntryAsset) == false || soundEntryAsset.tag == default) continue;
                tagToSoundEntry.TryAdd(soundEntryAsset.tag, soundEntryAsset);
            }
        }

#if UNITY_EDITOR
        [EditorButton("Update")]
        public void UpdateAnimationEntries()
        {
            sounds.Clear();
            var myPath = AssetDatabase.GetAssetPath(this);
            myPath = System.IO.Path.GetDirectoryName(myPath);
            var soundEntryGuids = AssetDatabase.FindAssets($"t:{nameof(SoundEntry)}", new string[] { myPath });
            
            Undo.RecordObject(this, "Updated Soundbank Entries");
            foreach (var guid in soundEntryGuids)
            {
                var asset = AssetDatabase.LoadAssetByGUID<SoundEntry>(new UnityEngine.GUID(guid));
                if (asset == null || !asset.assignedSoundbanks.Contains(tag)) continue;
                sounds.Add(asset);
            }
            EditorUtility.SetDirty(this);
        }
#endif
    }
}