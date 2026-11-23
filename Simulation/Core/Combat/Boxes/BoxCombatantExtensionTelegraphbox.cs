namespace Quantum
{
    public unsafe partial struct BoxCombatant
    {
        public bool TelegraphboxExistWithId(Frame frame, int boxID)
        {
            var list = frame.ResolveList(telegraphboxList);

            for (int i = 0; i < list.Count; i++)
            {
                if (!frame.Unsafe.TryGetPointer<Telegraphbox>(list[i], out var h)) continue;
                if (h->id != boxID) continue;
                return true;
            }

            return false;
        }

        public bool TryGetTelegraphbox(Frame frame, int boxID, out int boxIndex)
        {
            var list = frame.ResolveList(telegraphboxList);

            for (int i = 0; i < list.Count; i++)
            {
                if(!frame.Unsafe.TryGetPointer<Telegraphbox>(list[i], out var h)) continue;
                if (h->id != boxID) continue;
                boxIndex = i;
                return true;
            }

            boxIndex = -1;
            return false;
        }

        public void DeleteTelegraphboxByID(Frame frame, int boxID)
        {
            var list = frame.ResolveList(telegraphboxList);

            for (int i = 0; i < list.Count; i++)
            {
                if(!frame.Unsafe.TryGetPointer<Telegraphbox>(list[i], out var h)) continue;
                if (h->id != boxID) continue;
                frame.Destroy(list[i]);
                list.RemoveAt(i);
                return;
            }
        }

        public void CleanupTelegraphboxes(Frame frame)
        {
            var list = frame.ResolveList(telegraphboxList);

            while (list.Count > 0)
            {
                frame.Destroy(list[list.Count - 1]);
                list.RemoveAt(list.Count - 1);
            }
        }
    }
}