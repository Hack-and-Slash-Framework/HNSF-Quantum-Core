using System.Collections.Generic;
using Quantum;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HnSF
{
    public static class GameplayTagsSourceExtension
    {
        public static List<Tag> GetBaseTags(this GameplayTagsSource self)
        {
            List<Tag> baseTags = new List<Tag>();
            for (int i = 0; i < self.GameplayTags.Count; i++)
            {
                if(self.GameplayTags[i] == default) continue;
                var tag = QuantumUnityDB.GetGlobalAssetEditorInstance(self.GameplayTags[i]);
                if (tag == null || tag.parent != null) continue;
                baseTags.Add(tag);
            }

            return baseTags;
        }
        
#if UNITY_EDITOR
        public static bool TryCreateTag(this GameplayTagsSource self, string tagName)
        {
            var tagParts = tagName.Split('.');
            for (int i = 0; i < tagParts.Length; i++)
            {
                if (string.IsNullOrEmpty(tagParts[i])) return false;
            }
            if (tagParts.Length == 0) return false;
            var baseTags = self.GetBaseTags();

            int matchedParts = 0;
            Tag branchAsset = null;
            for (int i = 0; i < baseTags.Count; i++)
            {
                if (baseTags[i].label != tagParts[0]) continue;
                matchedParts++;
                branchAsset = baseTags[i];
                self.FindBranchRecursive(baseTags[i], tagParts, ref branchAsset, ref matchedParts);
                break;
            }
            Debug.Log($"Found {matchedParts}, got branch of {branchAsset?.label}");

            if (branchAsset == null)
            {
                branchAsset = ScriptableObject.CreateInstance<Tag>();
                branchAsset.label = tagParts[0];
                AssetDatabase.CreateAsset(branchAsset, $"{self.newTagLocation}{tagParts[0]}.asset");
                self.GameplayTags.Add(branchAsset);
                EditorUtility.SetDirty(self);
                matchedParts = 1;
            }

            self.CreateTagsRecursive(branchAsset, tagParts, ref matchedParts);
            
            self.OnTagCreated.Invoke();
            return true;
        }
        
        public static void FindBranchRecursive(this GameplayTagsSource self, Tag currTag, string[] tagParts, ref Tag branchAsset, ref int matchedParts)
        {
            if(matchedParts >= tagParts.Length) return;

            for (int i = 0; i < currTag.childTags.Count; i++)
            {
                var childTag = QuantumUnityDB.GetGlobalAssetEditorInstance<Tag>(currTag.childTags[i]);
                if(childTag.label != tagParts[matchedParts]) continue;
                matchedParts++;
                branchAsset = childTag;
                self.FindBranchRecursive(childTag, tagParts, ref branchAsset, ref matchedParts);
                break;
            }
        }

        public static void CreateTagsRecursive(this GameplayTagsSource self, Tag branchAsset, string[] tagParts, ref int matchedParts)
        {
            if (matchedParts >= tagParts.Length) return;
            for (int i = 0; i < branchAsset.childTags.Count; i++)
            {
                var childTag = QuantumUnityDB.GetGlobalAssetEditorInstance<Tag>(branchAsset.childTags[i]);
                if(childTag.label != tagParts[matchedParts]) continue;
                matchedParts++;
                self.CreateTagsRecursive(childTag, tagParts, ref matchedParts);
                return;
            }
            
            // No Tag found, create the next one.
            var childAsset = ScriptableObject.CreateInstance<Tag>();
            childAsset.label = tagParts[matchedParts];
            childAsset.parent = branchAsset;
            AssetDatabase.CreateAsset(childAsset, $"{self.newTagLocation}{branchAsset.name}.{tagParts[matchedParts]}.asset");
            self.GameplayTags.Add(childAsset);
            EditorUtility.SetDirty(self);
            branchAsset.childTags.Add(childAsset);
            EditorUtility.SetDirty(branchAsset);
            matchedParts++;

            if (matchedParts >= tagParts.Length) return;
            self.CreateTagsRecursive(childAsset, tagParts, ref matchedParts);
        }
#endif
    }
}