namespace Quantum
{
    public unsafe partial struct BoxCombatant
    {
        public bool HurtboxExistWithId(Frame frame, int hitboxID)
        {
            var list = frame.ResolveList(hurtboxList);

            for (int i = 0; i < list.Count; i++)
            {
                if (!frame.Unsafe.TryGetPointer<Hurtbox>(list[i], out var h)) continue;
                if (h->id != hitboxID) continue;
                return true;
            }

            return false;
        }

        public bool TryGetHurtbox(Frame frame, int hurtboxID, out int hurtboxIndex)
        {
            var list = frame.ResolveList(hurtboxList);

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

        public bool TryGetHurtbox(Frame frame, int hurtboxID, out EntityRef hurtbox)
        {
            var list = frame.ResolveList(hurtboxList);

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

        public void DeleteHurtboxByID(Frame frame, int hurtboxID)
        {
            var list = frame.ResolveList(hurtboxList);

            for (int i = 0; i < list.Count; i++)
            {
                if (!frame.TryGet(list[i], out Hurtbox h)) continue;
                if (h.id != hurtboxID) continue;
                frame.Destroy(list[i]);
                list.RemoveAt(i);
                return;
            }
        }

        public void CleanupHurtboxes(Frame frame)
        {
            var list = frame.ResolveList(hurtboxList);

            while (list.Count > 0)
            {
                frame.Destroy(list[list.Count - 1]);
                list.RemoveAt(list.Count - 1);
            }
        }
    }
}