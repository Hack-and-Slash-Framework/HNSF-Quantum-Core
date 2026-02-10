namespace Quantum
{
    public static unsafe partial class BoxCombatantHelper
    {
        public static unsafe partial class HitboxHelper
        {
            public static bool TryGetHitbox(Frame frame, BoxCombatant* boxCombatant, int hitboxID, out int hitboxIndex)
            {
                var list = frame.ResolveList(boxCombatant->hitboxList);

                for (int i = 0; i < list.Count; i++)
                {
                    if (!frame.TryGet(list[i], out Hitbox h)) continue;
                    if (h.id != hitboxID) continue;
                    hitboxIndex = i;
                    return true;
                }

                hitboxIndex = -1;
                return false;
            }
            
            public static bool TryGetHitbox(Frame frame, BoxCombatant* boxCombatant, int hitboxID, out EntityRef hitbox)
            {
                var list = frame.ResolveList(boxCombatant->hitboxList);

                foreach (var t in list)
                {
                    if (!frame.TryGet(t, out Hitbox h)) continue;
                    if (h.id != hitboxID) continue;
                    hitbox = t;
                    return true;
                }
                hitbox = default;
                return false;
            }

            public static void DeleteHitboxByID(Frame frame, BoxCombatant* boxCombatant, int hitboxID)
            {
                var list = frame.ResolveList(boxCombatant->hitboxList);
        
                for (int i = 0; i < list.Count; i++)
                {
                    if (!frame.TryGet(list[i], out Hitbox h)) continue;
                    if (h.id != hitboxID) continue;
                    frame.Destroy(list[i]);
                    list.RemoveAt(i);
                    return;
                }
            }

            public static void CleanupHitboxes(Frame frame, BoxCombatant* boxCombatant)
            {
                var list = frame.ResolveList(boxCombatant->hitboxList);
        
                while(list.Count > 0)
                {
                    frame.Destroy(list[^1]);
                    list.RemoveAt(list.Count-1);
                }
            }
        }
    }
}