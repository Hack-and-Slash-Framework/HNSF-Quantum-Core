namespace Quantum
{
    public unsafe partial struct HNSFEventReceiverGroup
    {
        public void Initialize(Frame frame, long key)
        {
            var actionsDict = frame.ResolveDictionary(actions);

            if (!actionsDict.ContainsKey(key))
            {
                var era = new EventReceiverActions();
                era.actions = frame.AllocateList<AssetRef<HNSFEventAction>>();
                
                actionsDict.Add(key, era);
            }
        }

        public void RegisterAction(Frame frame, long key, AssetRef<HNSFEventAction> eventActionAssetRef)
        {
            var actionsDict = frame.ResolveDictionary(actions);
            actionsDict.TryGetValuePointer(key, out var era);

            var act = frame.ResolveList(era->actions);
            
            act.Add(eventActionAssetRef);
        }
        
        public void RegisterActions(Frame frame, long key, AssetRef<HNSFEventAction>[] eventActionAssetRefs)
        {
            var actionsDict = frame.ResolveDictionary(actions);
            actionsDict.TryGetValuePointer(key, out var era);

            var act = frame.ResolveList(era->actions);

            foreach (var eaar in eventActionAssetRefs)
            {
                act.Add(eaar);
            }
        }

        public void UnregisterActions(Frame frame, long key)
        {
            var actionsDict = frame.ResolveDictionary(actions);
            if (!actionsDict.TryGetValuePointer(key, out var era)) return;
            frame.FreeList(ref era->actions);
            actionsDict.Remove(key);
        }
        
        public void UnregisterAction(Frame frame, long key, AssetRef<HNSFEventAction> eventActionAssetRefs)
        {
            var actionsDict = frame.ResolveDictionary(actions);
            if (!actionsDict.TryGetValuePointer(key, out var era)) return;
            var act = frame.ResolveList(era->actions);

            act.Remove(eventActionAssetRefs);

            if (act.Count == 0)
            {
                frame.FreeList(ref era->actions);
                actionsDict.Remove(key);
            }
        }
        
        public void UnregisterActions(Frame frame, long key, AssetRef<HNSFEventAction>[] eventActionAssetRefs)
        {
            var actionsDict = frame.ResolveDictionary(actions);
            if (!actionsDict.TryGetValuePointer(key, out var era)) return;
            var act = frame.ResolveList(era->actions);

            foreach (var eaar in eventActionAssetRefs) act.Remove(eaar);

            if (act.Count == 0)
            {
                frame.FreeList(ref era->actions);
                actionsDict.Remove(key);
            }
        }
    }
}
