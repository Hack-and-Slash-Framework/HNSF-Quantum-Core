namespace Quantum
{
    public static unsafe partial class BoxCombatantHelper
    {
        public static unsafe partial class HurtboxHelper
        {
            public static bool TryGetHurtbox(Frame frame, BoxCombatant* boxCombatant, int hurtboxID, out int hurtboxIndex)
            {
                var list = frame.ResolveList(boxCombatant->hurtboxList);

                for (int i = 0; i < list.Count; i++)
                {
                    if (!frame.TryGet(list[i], out Hurtbox h)) continue;
                    if (h.id != hurtboxID) continue;
                    hurtboxIndex = i;
                    return true;
                }

                hurtboxIndex = -1;
                return false;
            }
            
            public static bool TryGetHurtbox(Frame frame, BoxCombatant* boxCombatant, int hurtboxID, out EntityRef hurtbox)
            {
                var list = frame.ResolveList(boxCombatant->hurtboxList);

                foreach (var t in list)
                {
                    if (!frame.TryGet(t, out Hurtbox h)) continue;
                    if (h.id != hurtboxID) continue;
                    hurtbox = t;
                    return true;
                }
                hurtbox = default;
                return false;
            }

            public static void DeleteHurtboxByID(Frame frame, BoxCombatant* boxCombatant, int hurtboxID)
            {
                var list = frame.ResolveList(boxCombatant->hurtboxList);
        
                for (int i = 0; i < list.Count; i++)
                {
                    if (!frame.TryGet(list[i], out Hurtbox h)) continue;
                    if (h.id != hurtboxID) continue;
                    frame.Destroy(list[i]);
                    list.RemoveAt(i);
                    return;
                }
            }

            public static void CleanupHurtboxes(Frame frame, BoxCombatant* boxCombatant)
            {
                var list = frame.ResolveList(boxCombatant->hurtboxList);
        
                while(list.Count > 0)
                {
                    frame.Destroy(list[^1]);
                    list.RemoveAt(list.Count-1);
                }
            }
        }
    }
}