namespace Quantum
{
    public unsafe partial struct BoxCombatant
    {
        public bool ThrowboxExistWithId(Frame frame, int throwboxID)
        {
            var list = frame.ResolveList(throwboxList);

            for (int i = 0; i < list.Count; i++)
            {
                if (!frame.Unsafe.TryGetPointer<Throwbox>(list[i], out var h)) continue;
                if (h->id != throwboxID) continue;
                return true;
            }

            return false;
        }

        public bool TryGetThrowbox(Frame frame, int throwboxID, out int throwboxIndex)
        {
            var list = frame.ResolveList(throwboxList);

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

        public void DeleteThrowboxByID(Frame frame, int throwboxID)
        {
            var list = frame.ResolveList(throwboxList);

            for (int i = 0; i < list.Count; i++)
            {
                if (!frame.TryGet(list[i], out Throwbox h)) continue;
                if (h.id != throwboxID) continue;
                frame.Destroy(list[i]);
                list.RemoveAt(i);
                return;
            }
        }

        public void CleanupThrowboxes(Frame frame)
        {
            var list = frame.ResolveList(throwboxList);

            while (list.Count > 0)
            {
                frame.Destroy(list[list.Count - 1]);
                list.RemoveAt(list.Count - 1);
            }
        }
    }
}