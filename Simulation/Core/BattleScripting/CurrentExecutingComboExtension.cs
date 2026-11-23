namespace Quantum
{
    public unsafe partial struct CurrentExecutingCombo
    {
        public void Cleanup(Frame frame, EntityRef groupKey, bool destroyEntity)
        {
            var genericControlManager = frame.GetOrAddSingleton<GenericGroupControlManager>();
            genericControlManager.Remove(frame, groupKey, scriptEntityRef, destroyEntity);
        }

        public void ClearCombo(Frame frame)
        {
            comboAssetRef = default;
            comboIndex = -1;
            status = ExecutingComboStatus.None;
        }
    }
}