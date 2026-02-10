using Quantum;

namespace HnSF.core
{
    public static unsafe partial class CombatHelper
    {
        public static unsafe partial class ThrowHelper
        {
            public static void GrabEntity(Frame frame, EntityRef self, EntityRef entityToGrab)
            {
                var isInThrow = new IsBeingThrown(){ thrower = self };
                frame.Add(entityToGrab, isInThrow);

                frame.AddOrGet<IsThrowing>(self, out var isThrowing);
                var throweesDict = frame.ResolveDictionary(isThrowing->throwees);
                throweesDict.TryAdd(0, entityToGrab);
            }
            
            public static void ReleaseThrowee(Frame f, EntityRef selfEntityRef, int throweeId)
            {
                if (!f.Unsafe.TryGetPointer<IsThrowing>(selfEntityRef, out var isThrowing)) return;
                var dict = f.ResolveDictionary(isThrowing->throwees);

                if (dict.ContainsKey(throweeId))
                {
                    f.Remove<IsBeingThrown>(dict[throweeId]);
                    dict.Remove(throweeId);
                }

                if (dict.Count == 0) f.Remove<IsThrowing>(selfEntityRef);
            }
            
            public static void ReleaseThrowee(Frame f, EntityRef selfEntityRef, EntityRef throwee)
            {
                if (!f.Unsafe.TryGetPointer<IsThrowing>(selfEntityRef, out var isThrowing)) return;
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
                if (dict.Count == 0) f.Remove<IsThrowing>(selfEntityRef);
            }
            
            public static void ReleaseAllThrowees(Frame f, EntityRef selfEntityRef)
            {
                if (!f.Unsafe.TryGetPointer<IsThrowing>(selfEntityRef, out var isThrowing)) return;
                var dict = f.ResolveDictionary(isThrowing->throwees);

                foreach (var kv in dict)
                {
                    f.Remove<IsBeingThrown>(kv.Value);
                    dict.Remove(kv.Key);
                }
            
                dict.Clear();
            
                f.Remove<IsThrowing>(selfEntityRef);
            }
            
            public static void EscapeThrow(Frame frame, EntityRef entityRef)
            {
                if (!frame.Unsafe.TryGetPointer<IsBeingThrown>(entityRef, out var isInThrow)) return;
                ReleaseThrowee(frame, isInThrow->thrower, entityRef);
            }
        }
    }
}
