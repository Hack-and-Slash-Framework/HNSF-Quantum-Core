namespace Quantum
{
    public unsafe partial struct ScriptsControllingEntity
    {
        public bool Remove(Frame frame, EntityRef entityRef)
        {
            var list = frame.ResolveList(scriptEntityList);
            list.Remove(entityRef);
            return list.Count == 0;
        }
        
        public void Cleanup(Frame frame)
        {
            frame.FreeList(scriptEntityList);
            scriptEntityList = default;
        }
    }
}