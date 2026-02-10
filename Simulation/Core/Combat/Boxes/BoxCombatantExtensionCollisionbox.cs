namespace Quantum
{
    public unsafe partial struct BoxCombatant
    {
        public bool CollisionboxExistWithId(Frame frame, int hitboxID)
        {
            var list = frame.ResolveList(collisionboxList);

            for (int i = 0; i < list.Count; i++)
            {
                if (!frame.Unsafe.TryGetPointer<Collisionbox>(list[i], out var h)) continue;
                if (h->id != hitboxID) continue;
                return true;
            }

            return false;
        }

        public bool TryGetCollisionbox(Frame frame, int collisionboxID, out int collisionboxIndex)
        {
            var list = frame.ResolveList(collisionboxList);

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

        public void DeleteCollisionboxByID(Frame frame, int hitboxID)
        {
            var list = frame.ResolveList(collisionboxList);

            for (int i = 0; i < list.Count; i++)
            {
                if (!frame.TryGet(list[i], out Collisionbox h)) continue;
                if (h.id != hitboxID) continue;
                frame.Destroy(list[i]);
                list.RemoveAt(i);
                return;
            }
        }

        public void CleanupCollisionboxes(Frame frame)
        {
            var list = frame.ResolveList(collisionboxList);

            while (list.Count > 0)
            {
                frame.Destroy(list[list.Count - 1]);
                list.RemoveAt(list.Count - 1);
            }
        }
    }
}