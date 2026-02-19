using System.Collections.Generic;

namespace Quantum
{
    public unsafe partial struct GameplayTagCountContainer
    {
        public bool AddTagUnique(Frame frame, AssetRef tag)
        {
            var tagDictionary = frame.ResolveDictionary(tags);
            if (tagDictionary.ContainsKey(tag)) return false;
            tagDictionary.Add(tag, 0);
            return true;
        }
        
        public bool AddTag(Frame frame, AssetRef tag)
        {
            var tagDictionary = frame.ResolveDictionary(tags);
            if (tagDictionary.ContainsKey(tag) == false) tagDictionary.Add(tag, 0);
            tagDictionary[tag] += 1;
            return true;
        }
        
        public bool HasTag(Frame frame, AssetRef tag)
        {
            var tagDictionary = frame.ResolveDictionary(tags);
            return tagDictionary.ContainsKey(tag);
        }

        public bool HasAny(Frame frame, List<AssetRef> validTags)
        {
            var tagDictionary = frame.ResolveDictionary(this.tags);
            foreach (var tag in validTags)
            {
                if (tagDictionary.ContainsKey(tag)) return true;
            }
            return false;
        }
        
        public int GetTagCount(Frame frame, AssetRef tag)
        {
            var tagDictionary = frame.ResolveDictionary(this.tags);
            if (tagDictionary.ContainsKey(tag)) return tagDictionary[tag];
            return 0;
        }
    }
}