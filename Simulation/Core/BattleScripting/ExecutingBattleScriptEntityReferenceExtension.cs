namespace Quantum
{
    public unsafe partial struct ExecutingBattleScriptEntityReference
    {
        public void Cleanup(Frame frame, EntityRef groupKey, bool destroyEntity)
        {
            var genericControlManager = frame.GetOrAddSingleton<GenericGroupControlManager>();
            genericControlManager.Remove(frame, groupKey, entityRef, destroyEntity);
        }
    }
}
