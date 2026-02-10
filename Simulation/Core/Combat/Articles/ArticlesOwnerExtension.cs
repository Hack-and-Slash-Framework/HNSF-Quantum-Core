namespace Quantum
{
    public unsafe partial struct ArticlesOwner
    {
        public EntityRef SpawnArticle(Frame f, EntityRef owner, AssetRef<EntityPrototype> articlePrototype)
        {
            var list = f.ResolveList(articleRefs);

            var entityRef = f.Create(articlePrototype);

            if (f.Unsafe.TryGetPointer<Article>(entityRef, out var fArticle))
            {
                fArticle->owner = owner;
            }
        
            list.Add(entityRef);
            return entityRef;
        }

        public bool TryGetLastArticle(Frame f, int offset, out EntityRef articleEntityRef)
        {
            articleEntityRef = default;
            var list = f.ResolveList(articleRefs);
            if (list.Count <= offset) return false;
            articleEntityRef = list[list.Count-1-offset];
            return true;
        }

        public void RemoveArticle(Frame f, EntityRef articleEntityRef)
        {
            var list = f.ResolveList(articleRefs);
            list.Remove(articleEntityRef);
        }
    }
}