namespace HnSF.StatusEffects
{
    [System.Serializable]
    public enum StackExpirationPolicy
    {
        RemoveSingleStackAndRefresh,
        ClearEntireStack,
        RefreshDuration
    }
}