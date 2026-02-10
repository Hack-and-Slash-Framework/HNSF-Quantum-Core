namespace Quantum
{
    public unsafe partial struct Article
    {
        public void DestroyArticle(Frame f, EntityRef articleEntityRef)
        {
            if (f.Unsafe.TryGetPointer<ArticlesOwner>(owner, out var articlesOwner))
            {
                articlesOwner->RemoveArticle(f, articleEntityRef);
            }
            f.Destroy(articleEntityRef);
        }
        
        public static EntityRef GetRootOwner(Frame frame, EntityRef selfEntityRef)
        {
            if (!frame.Has<Article>(selfEntityRef)) return EntityRef.None;
            
            var currentWorkingEntity = selfEntityRef;
            
            while (currentWorkingEntity != EntityRef.None)
            {
                if (!frame.Unsafe.TryGetPointer<Article>(currentWorkingEntity, out var article)
                    || article->owner == EntityRef.None 
                    || !frame.Exists(article->owner)) return currentWorkingEntity;
                currentWorkingEntity = article->owner;
            }
            return EntityRef.None;
        }
    }
}
