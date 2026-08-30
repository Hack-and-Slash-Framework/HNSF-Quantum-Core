namespace Quantum
{
    public unsafe partial struct GenericGroupControlManager
    {
        public void Add(Frame frame, EntityRef keyEntityRef, EntityRef valueEntityRef)
        {
            var dict = frame.ResolveDictionary(controlInfoEntityMap);

            if (!dict.ContainsKey(keyEntityRef))
            {
                dict.Add(keyEntityRef, new ScriptsControllingEntity
                {
                    scriptEntityList = frame.AllocateList<EntityRef>()
                });
            }

            if (!dict.TryGetValuePointer(keyEntityRef, out var sce))
                return;
            var sList = frame.ResolveList(sce->scriptEntityList);
            sList.Add(valueEntityRef);
        }
        
        public void Add(Frame frame, AssetRef keyAssetRef, EntityRef valueEntityRef)
        {
            var dict = frame.ResolveDictionary(controlInfoEntityAssetRefMap);

            if (!dict.ContainsKey(keyAssetRef))
            {
                dict.Add(keyAssetRef, new ScriptsControllingEntity
                {
                    scriptEntityList = frame.AllocateList<EntityRef>()
                });
            }

            if (!dict.TryGetValuePointer(keyAssetRef, out var sce))
                return;
            var sList = frame.ResolveList(sce->scriptEntityList);
            sList.Add(valueEntityRef);
        }

        public void Remove(Frame frame, AssetRef keyAssetRef, EntityRef valueEntityRef, bool destroyEntity)
        {
            var dict = frame.ResolveDictionary(controlInfoEntityAssetRefMap);
            if (!dict.ContainsKey(keyAssetRef))
                return;
            
            if(dict[keyAssetRef].Remove(frame, valueEntityRef))
                RemoveGroup(frame, keyAssetRef);
            
            if (destroyEntity)
                frame.Destroy(valueEntityRef);
        }
        
        public void Remove(Frame frame, EntityRef keyEntityRef, EntityRef valueEntityRef, bool destroyEntity)
        {
            var dict = frame.ResolveDictionary(controlInfoEntityMap);
            if (!dict.ContainsKey(keyEntityRef))
                return;
            
            if(dict[keyEntityRef].Remove(frame, valueEntityRef))
                RemoveGroup(frame, keyEntityRef);
            
            if (destroyEntity)
                frame.Destroy(valueEntityRef);
        }
        
        public void RemoveGroup(Frame frame, AssetRef keyAssetRef)
        {
            var dict = frame.ResolveDictionary(controlInfoEntityAssetRefMap);
            if (!dict.TryGetValuePointer(keyAssetRef, out var sce))
                return;
            frame.FreeList(sce->scriptEntityList);
            dict.Remove(keyAssetRef);
        }
        
        public void RemoveGroup(Frame frame, EntityRef keyEntityRef)
        {
            var dict = frame.ResolveDictionary(controlInfoEntityMap);
            if (!dict.TryGetValuePointer(keyEntityRef, out var sce))
                return;
            frame.FreeList(sce->scriptEntityList);
            dict.Remove(keyEntityRef);
        }

        public bool ContainsKey(Frame frame, AssetRef keyAssetRef)
        {
            var dict = frame.ResolveDictionary(controlInfoEntityAssetRefMap);
            return dict.ContainsKey(keyAssetRef);
        }
        
        public bool ContainsKey(Frame frame, EntityRef keyEntityRef)
        {
            var dict = frame.ResolveDictionary(controlInfoEntityMap);
            return dict.ContainsKey(keyEntityRef);
        }
    }
}