namespace Quantum
{
    [System.Serializable]
    public enum StateActionTargetType
    {
        Self,
        Throwee,
        LockOnTarget,
        ArticleOwner,
        ArticleOwnerRoot,
        SoftTarget,
        LastCreatedArticle,
        FromEntityMap,
        LastHitEntity,
        LastHitByEntity,
        FromFunction
    }
}
