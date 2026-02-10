namespace Quantum
{
    public static unsafe partial class BoxCombatantHelper
    {
        public static unsafe partial class ThrowboxHelper
        {
            public static bool TryGetThrowbox(Frame frame, BoxCombatant* boxCombatant, int throwboxID, out int throwboxIndex)
            {
                var list = frame.ResolveList(boxCombatant->throwboxList);

                for (int i = 0; i < list.Count; i++)
                {
                    if (!frame.TryGet(list[i], out Throwbox h)) continue;
                    if (h.id != throwboxID) continue;
                    throwboxIndex = i;
                    return true;
                }

                throwboxIndex = -1;
                return false;
            }
            
            public static bool TryGetThrowbox(Frame frame, BoxCombatant* boxCombatant, int throwboxID, out EntityRef throwbox)
            {
                var list = frame.ResolveList(boxCombatant->throwboxList);

                foreach (var t in list)
                {
                    if (!frame.TryGet(t, out Throwbox h)) continue;
                    if (h.id != throwboxID) continue;
                    throwbox = t;
                    return true;
                }
                throwbox = default;
                return false;
            }

            public static void DeleteThrowboxByID(Frame frame, BoxCombatant* boxCombatant, int throwboxID)
            {
                var list = frame.ResolveList(boxCombatant->throwboxList);
        
                for (int i = 0; i < list.Count; i++)
                {
                    if (!frame.TryGet(list[i], out Throwbox h)) continue;
                    if (h.id != throwboxID) continue;
                    frame.Destroy(list[i]);
                    list.RemoveAt(i);
                    return;
                }
            }

            public static void CleanupThrowboxes(Frame frame, BoxCombatant* boxCombatant)
            {
                var list = frame.ResolveList(boxCombatant->throwboxList);
        
                while(list.Count > 0)
                {
                    frame.Destroy(list[^1]);
                    list.RemoveAt(list.Count-1);
                }
            }
        }
    }
}