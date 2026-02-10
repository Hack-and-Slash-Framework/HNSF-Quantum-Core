namespace Quantum
{
    public unsafe partial struct BoxCombatant
    {
        public bool HitboxExistWithId(Frame frame, int hitboxID)
        {
            var list = frame.ResolveList(hitboxList);

            for (int i = 0; i < list.Count; i++)
            {
                if (!frame.Unsafe.TryGetPointer<Hitbox>(list[i], out var h)) continue;
                if (h->id != hitboxID) continue;
                return true;
            }

            return false;
        }

        public bool TryGetHitbox(Frame frame, int hitboxID, out int hitboxIndex)
        {
            var list = frame.ResolveList(hitboxList);

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

        public void DeleteHitboxByID(Frame frame, int hitboxID)
        {
            var list = frame.ResolveList(hitboxList);

            for (int i = 0; i < list.Count; i++)
            {
                if (!frame.TryGet(list[i], out Hitbox h)) continue;
                if (h.id != hitboxID) continue;
                frame.Destroy(list[i]);
                list.RemoveAt(i);
                return;
            }
        }

        public void CleanupHitboxes(Frame frame)
        {
            var list = frame.ResolveList(hitboxList);

            while (list.Count > 0)
            {
                frame.Destroy(list[list.Count - 1]);
                list.RemoveAt(list.Count - 1);
            }
        }
    }
}