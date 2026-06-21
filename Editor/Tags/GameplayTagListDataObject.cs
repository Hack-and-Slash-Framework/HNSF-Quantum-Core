using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HnSF
{
    [System.Serializable]
    public class GameplayTagListDataObject : ScriptableObject
    {
        [System.Serializable]
        public class TagReferenceData
        {
            public int id;
            public string propertyPath;
            [NonSerialized] public SerializedProperty sp;
            public bool selected;
            public Quantum.Tag tagAsset;
            [SerializeReference] public TagReferenceData parent;
            [SerializeReference] public TagReferenceData[] childTags;

            public void SelectToRoot()
            {
                selected = true;
                parent?.SelectToRoot();
            }

            public void DeselectChildren()
            {
                selected = false;
                for (int i = 0; i < childTags.Length; i++)
                {
                    childTags[i].DeselectChildren();
                }
            }
        }

        [SerializeReference] public List<TagReferenceData> tagMap = new List<TagReferenceData>();
        [SerializeReference] public List<TagReferenceData> selectedTags = new List<TagReferenceData>();

        public void EvaluateSelectedTags()
        {
            selectedTags.Clear();
            List<TagReferenceData> leafTags = new List<TagReferenceData>();
            for (int i = 0; i < tagMap.Count; i++)
            {
                if (tagMap[i].selected == false) continue;
                DiscoverSelectedLeafs(tagMap[i], leafTags);
            }
            selectedTags = leafTags;

            //PrintSelectedTags();
        }

        private void PrintSelectedTags()
        {
            string prt = "Got Tags:\n";
            for (int i = 0; i < selectedTags.Count; i++)
            {
                prt += selectedTags[i].tagAsset.label + ", ";
            }
            Debug.Log(prt);
        }

        private bool DiscoverSelectedLeafs(TagReferenceData currentTag, List<TagReferenceData> leafTags)
        {
            if (currentTag.selected == false) return false;
            bool leafHasSelection = false;
            for (int i = 0; i < currentTag.childTags.Length; i++)
            {
                if (DiscoverSelectedLeafs(currentTag.childTags[i], leafTags)) leafHasSelection = true;
            }

            if (leafHasSelection == false) leafTags.Add(currentTag);
            return true;
        }
    }
}