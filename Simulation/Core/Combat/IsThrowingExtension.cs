namespace Quantum
{
    public unsafe partial struct IsThrowing
    {
        public void ReleaseThrowee(Frame f, EntityRef entity, int throweeId)
        {
            if (!f.Unsafe.TryGetPointer<IsThrowing>(entity, out var isThrowing)) return;
            var dict = f.ResolveDictionary(isThrowing->throwees);

            if (dict.ContainsKey(throweeId))
            {
                f.Remove<IsBeingThrown>(dict[throweeId]);
                dict.Remove(throweeId);
            }

            if (dict.Count == 0) f.Remove<IsThrowing>(entity);
        }

        public void ReleaseThrowee(Frame f, EntityRef entity, EntityRef throwee)
        {
            if (!f.Unsafe.TryGetPointer<IsThrowing>(entity, out var isThrowing)) return;
            var dict = f.ResolveDictionary(isThrowing->throwees);

            var idx = -1;
            foreach (var kv in dict)
            {
                if (kv.Value != throwee) continue;
                idx = kv.Key;
                break;
            }

            if (idx == -1) return;
            f.Remove<IsBeingThrown>(dict[idx]);
            dict.Remove(idx);
            if (dict.Count == 0) f.Remove<IsThrowing>(entity);
        }

        public void ReleaseAllThrowees(Frame f, EntityRef entity)
        {
            if (!f.Unsafe.TryGetPointer<IsThrowing>(entity, out var isThrowing)) return;
            var dict = f.ResolveDictionary(isThrowing->throwees);

            foreach (var kv in dict)
            {
                f.Remove<IsBeingThrown>(kv.Value);
                dict.Remove(kv.Key);
            }
            
            dict.Clear();
            
            f.Remove<IsThrowing>(entity);
        }
    }
}