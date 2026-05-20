using System.Collections.Generic;

namespace Quantum
{
    public unsafe partial struct GameplayTagContainer
    {
        public bool AddTagUnique(Frame frame, AssetRef tag)
        {
            var tagList = frame.ResolveList(tags);
            if (tagList.Contains(tag)) return false;
            tagList.Add(tag);
            AddImplicitTags(frame, tag);
            return true;
        }

        public bool AddTag(Frame frame, AssetRef tag)
        {
            var tagList = frame.ResolveList(tags);
            tagList.Add(tag);
            AddImplicitTags(frame, tag);
            return true;
        }

        public void RemoveTag(Frame frame, AssetRef tag)
        {
            var tagList = frame.ResolveList(tags);
            if (tagList.Remove(tag) == false) return;
            RemoveImplicitTags(frame, tag);
        }
        
        public bool HasTag(Frame frame, AssetRef tag)
        {
            var tagList = frame.ResolveList(tags);
            return tagList.Contains(tag);
        }

        public bool HasAny(Frame frame, List<AssetRef> validTags)
        {
            var tagList = frame.ResolveList(this.tags);
            foreach (var tag in validTags)
            {
                if (tagList.Contains(tag)) return true;
            }
            return false;
        }

        public int GetTagCount(Frame frame, AssetRef tag)
        {
            var tagList = frame.ResolveList(this.tags);
            int cnt = 0;
            foreach (var myTag in tagList)
            {
                if (myTag == tag) cnt++;
            }
            return cnt;
        }

        private void AddImplicitTags(Frame frame, AssetRef tag)
        {
            if (!frame.TryFindAsset(tag, out Tag tagAsset)) return;
            var implicitTagList = frame.ResolveList(implicitTags);
            
            var nextParent = tagAsset.parent;
            while (nextParent != null)
            {
                if (!frame.TryFindAsset(nextParent, out Tag nextTag)) break;
                implicitTagList.Add(nextTag);
                nextParent = nextTag.parent;
            }
        }

        private void RemoveImplicitTags(Frame frame, AssetRef tag)
        {
            if (!frame.TryFindAsset(tag, out Tag tagAsset)) return;
            var implicitTagList = frame.ResolveList(implicitTags);
            
            var nextParent = tagAsset.parent;
            while (nextParent != null)
            {
                if (!frame.TryFindAsset(nextParent, out Tag nextTag)) break;
                implicitTagList.Remove(nextTag);
                nextParent = nextTag.parent;
            }
        }
    }
}
