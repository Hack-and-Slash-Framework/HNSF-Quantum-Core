namespace Quantum
{
    public static unsafe partial class BoxCombatantHelper
    {
        public static unsafe partial class WarningboxHelper
        {
            public static bool TryGetWarningBox(Frame frame, BoxCombatant* boxCombatant, int warningboxID, out int warningboxIndex)
            {
                var list = frame.ResolveList(boxCombatant->warningboxList);

                for (int i = 0; i < list.Count; i++)
                {
                    if (!frame.TryGet(list[i], out Warningbox h)) continue;
                    if (h.id != warningboxID) continue;
                    warningboxIndex = i;
                    return true;
                }

                warningboxIndex = -1;
                return false;
            }
            
            public static bool TryGetWarningBox(Frame frame, BoxCombatant* boxCombatant, int warningboxID, out EntityRef hurtbox)
            {
                var list = frame.ResolveList(boxCombatant->warningboxList);

                foreach (var t in list)
                {
                    if (!frame.TryGet(t, out Warningbox h)) continue;
                    if (h.id != warningboxID) continue;
                    hurtbox = t;
                    return true;
                }
                hurtbox = default;
                return false;
            }

            public static void DeleteWarningboxByID(Frame frame, BoxCombatant* boxCombatant, int warningboxID)
            {
                var list = frame.ResolveList(boxCombatant->warningboxList);
        
                for (int i = 0; i < list.Count; i++)
                {
                    if (!frame.TryGet(list[i], out Warningbox h)) continue;
                    if (h.id != warningboxID) continue;
                    frame.Destroy(list[i]);
                    list.RemoveAt(i);
                    return;
                }
            }

            public static void CleanupWarningboxes(Frame frame, BoxCombatant* boxCombatant)
            {
                var list = frame.ResolveList(boxCombatant->warningboxList);
        
                while(list.Count > 0)
                {
                    frame.Destroy(list[^1]);
                    list.RemoveAt(list.Count-1);
                }
            }
        }
    }
}
