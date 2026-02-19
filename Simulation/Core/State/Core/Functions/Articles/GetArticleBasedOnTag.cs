using System.Collections.Generic;
using System.Linq;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class GetArticleBasedOnTag : StateFunctionEntityRef
    {
        public List<AssetRef> validTags = new List<AssetRef>();
        
        public override EntityRef Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if(!frame.Unsafe.TryGetPointer<ArticlesOwner>(entity, out var articlesOwner)) return EntityRef.None;
            var articles = frame.ResolveList(articlesOwner->articleRefs);

            foreach (var article in articles)
            {
                if (TagContainerHelper.HasAny(frame, article, validTags)) return article;
            }
            return EntityRef.None;
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new GetArticleBasedOnTag());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as GetArticleBasedOnTag;
            t.validTags = validTags.ToList();
            return base.CopyTo(target);
        }
    }
}