namespace Quantum
{
    public static unsafe partial class BoxCombatantHelper
    {
        public static unsafe partial class TelegraphboxHelper
        {
            public static bool TryGetTelegraphBox(Frame frame, BoxCombatant* boxCombatant, int boxID, out int boxIndex)
            {
                var list = frame.ResolveList(boxCombatant->telegraphboxList);

                for (int i = 0; i < list.Count; i++)
                {
                    if (!frame.TryGet(list[i], out Telegraphbox h)) continue;
                    if (h.id != boxID) continue;
                    boxIndex = i;
                    return true;
                }

                boxIndex = -1;
                return false;
            }
            
            public static bool TryGetTelegraphBox(Frame frame, BoxCombatant* boxCombatant, int boxID, out EntityRef hurtbox)
            {
                var list = frame.ResolveList(boxCombatant->telegraphboxList);

                foreach (var t in list)
                {
                    if (!frame.TryGet(t, out Telegraphbox h)) continue;
                    if (h.id != boxID) continue;
                    hurtbox = t;
                    return true;
                }
                hurtbox = default;
                return false;
            }

            public static void DeleteTelegraphboxByID(Frame frame, BoxCombatant* boxCombatant, int boxID)
            {
                var list = frame.ResolveList(boxCombatant->telegraphboxList);
        
                for (int i = 0; i < list.Count; i++)
                {
                    if (!frame.TryGet(list[i], out Telegraphbox h)) continue;
                    if (h.id != boxID) continue;
                    frame.Destroy(list[i]);
                    list.RemoveAt(i);
                    return;
                }
            }

            public static void CleanupTelegraphboxes(Frame frame, BoxCombatant* boxCombatant)
            {
                var list = frame.ResolveList(boxCombatant->telegraphboxList);
        
                while(list.Count > 0)
                {
                    frame.Destroy(list[^1]);
                    list.RemoveAt(list.Count-1);
                }
            }
        }
    }
}
