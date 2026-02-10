namespace Quantum
{
    public static unsafe partial class BoxCombatantHelper
    {
        public static unsafe partial class CollisionboxHelper
        {
            public static bool TryGetCollisionbox(Frame frame, BoxCombatant* boxCombatant, int collisionboxID, out int collisionboxIndex)
            {
                var list = frame.ResolveList(boxCombatant->collisionboxList);

                for (int i = 0; i < list.Count; i++)
                {
                    if (!frame.TryGet(list[i], out Collisionbox h)) continue;
                    if (h.id != collisionboxID) continue;
                    collisionboxIndex = i;
                    return true;
                }

                collisionboxIndex = -1;
                return false;
            }
            
            public static bool TryGetCollisionbox(Frame frame, BoxCombatant* boxCombatant, int collisionboxID, out EntityRef collisionbox)
            {
                var list = frame.ResolveList(boxCombatant->collisionboxList);

                foreach (var t in list)
                {
                    if (!frame.TryGet(t, out Collisionbox h)) continue;
                    if (h.id != collisionboxID) continue;
                    collisionbox = t;
                    return true;
                }
                collisionbox = default;
                return false;
            }

            public static void DeleteCollisionboxByID(Frame frame, BoxCombatant* boxCombatant, int collisionboxID)
            {
                var list = frame.ResolveList(boxCombatant->collisionboxList);
        
                for (int i = 0; i < list.Count; i++)
                {
                    if (!frame.TryGet(list[i], out Collisionbox h)) continue;
                    if (h.id != collisionboxID) continue;
                    frame.Destroy(list[i]);
                    list.RemoveAt(i);
                    return;
                }
            }

            public static void CleanupCollisionboxes(Frame frame, BoxCombatant* boxCombatant)
            {
                var list = frame.ResolveList(boxCombatant->collisionboxList);
        
                while(list.Count > 0)
                {
                    frame.Destroy(list[^1]);
                    list.RemoveAt(list.Count-1);
                }
            }
        }
    }
}