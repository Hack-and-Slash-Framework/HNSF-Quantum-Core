using System;
using System.Collections.Generic;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Articles/Destroy Article With Tag")]
    public unsafe partial class DestroyArticleWithTag : HNSFStateAction
    {
        public bool destroyAllWithTag;
        public List<AssetRef> validTags = new List<AssetRef>();
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (frame.Unsafe.TryGetPointer<ArticlesOwner>(entity, out var articlesOwner))
            {
                var articles = frame.ResolveList(articlesOwner->articleRefs);

                for (var index = 0; index < articles.Count; index++)
                {
                    var articleEntityRef = articles[index];
                    if (!frame.Unsafe.TryGetPointer<Tagged>(articleEntityRef, out var tagged)
                        || !validTags.Contains(tagged->tag)) continue;

                    articlesOwner->RemoveArticle(frame, articleEntityRef);
                    frame.Destroy(articleEntityRef);
                    break;
                }
            }
            return false;
        }
        
        public override HNSFStateAction Copy()
        {
            return CopyTo(new DestroyArticleWithTag());
        }
    }
}