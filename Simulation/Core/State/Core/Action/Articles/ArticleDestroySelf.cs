using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Articles/Article Destroy Self")]
    public unsafe partial class ArticleDestroySelf : HNSFStateAction
    {
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (frame.Unsafe.TryGetPointer<Article>(entity, out var selfArticle)
                && frame.Unsafe.TryGetPointer<ArticlesOwner>(selfArticle->owner, out var ownerArticles))
            {
                ownerArticles->RemoveArticle(frame, entity);
            }
            frame.Destroy(entity);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ArticleDestroySelf());
        }
    }
}