namespace Quantum
{
    public unsafe partial struct BoxCombatant
    {
        public bool HasHitEntity(Frame frame, EntityRef entityToCheck)
        {
            var list = frame.ResolveList(entitiesHit);

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].ent == entityToCheck) return true;
            }
            return false;
        }

        public int GetCurrentEntityHitCount(Frame frame)
        {
            var list = frame.ResolveList(entitiesHit);
            return list.Count;
        }
    
        public int GetCurrentEntityHitCount(Frame frame, int hitboxID)
        {
            var cnt = 0;
            var list = frame.ResolveList(entitiesHit);
            foreach (var eh in list)
            {
                if (eh.hitByHitboxIdentifier == hitboxID) cnt++;
            }
            return cnt;
        }

        public int GetTotalHitCount(Frame frame)
        {
            var hbcDictionary = frame.ResolveDictionary(hitReactionCounters);
            return hbcDictionary.ContainsKey((int)StandardHitReactions.Hit) ? hbcDictionary[(int)StandardHitReactions.Hit] : 0;
        }

        public int GetTotalBlockCount(Frame frame)
        {
            var hbcDictionary = frame.ResolveDictionary(hitReactionCounters);
            return hbcDictionary.ContainsKey((int)StandardHitReactions.Blocked) ? hbcDictionary[(int)StandardHitReactions.Blocked] : 0;
        }

        public int GetTotalHitOrBlockCount(Frame frame)
        {
            var hbcDictionary = frame.ResolveDictionary(hitReactionCounters);
            var hitCount = hbcDictionary.ContainsKey((int)StandardHitReactions.Hit)
                ? hbcDictionary[(int)StandardHitReactions.Hit]
                : 0;
            var blockCount = hbcDictionary.ContainsKey((int)StandardHitReactions.Blocked)
                ? hbcDictionary[(int)StandardHitReactions.Blocked]
                : 0;
            return hitCount + blockCount;
        }

        public void MarkEntityAsHit(Frame frame, EntityRef entityHit, int hitboxID)
        {
            var list = frame.ResolveList(entitiesHit);
            list.Add(new EntityHitDefinition(){ ent = entityHit, hitByHitboxIdentifier = hitboxID});
        }

        public void ClearHitList(Frame frame)
        {
            var list = frame.ResolveList(entitiesHit);
            list.Clear();
        }

        public void CleanupAllBoxes(Frame frame)
        {
            CleanupHurtboxes(frame);
            CleanupHitboxes(frame);
            CleanupCollisionboxes(frame);
            CleanupThrowboxes(frame);
            CleanupWarningboxes(frame);
        }
    }
}
