using System;
using System.Collections.Generic;
using System.Linq;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class HasArticleWithTag : HNSFStateDecision
    {
        public List<AssetRef> validTags = new List<AssetRef>();
        
        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<ArticlesOwner>(entity, out var articlesOwner)) return false;

            var articles = frame.ResolveList(articlesOwner->articleRefs);

            foreach (var article in articles)
            {
                if (TagContainerHelper.HasAny(frame, article, validTags)) return true;
            }
            return false;
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new HasArticleWithTag());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as HasArticleWithTag;
            t.validTags = validTags.ToList();
            return base.CopyTo(target);
        }
    }
}