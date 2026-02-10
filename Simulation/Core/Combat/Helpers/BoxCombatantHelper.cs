namespace Quantum
{
    public static unsafe partial class BoxCombatantHelper
    {
#region Touched
        public static void ClearTouchedEntities(Frame frame, BoxCombatant* boxCombatant)
        {
            var list = frame.ResolveList(boxCombatant->entitiesHit);
            list.Clear();
        }

        public static bool HasTouchedEntity(Frame frame, BoxCombatant* boxCombatant, EntityRef entityToCheck)
        {
            var list = frame.ResolveList(boxCombatant->entitiesHit);

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].ent == entityToCheck) return true;
            }
            return false;
        }
        
        public static int GetEntitiesTouchedCount(Frame frame, BoxCombatant* boxCombatant)
        {
            var list = frame.ResolveList(boxCombatant->entitiesHit);
            return list.Count;
        }
        
        public static int GetEntitiesTouchedByHitboxCount(Frame frame, BoxCombatant* boxCombatant, int hitboxID)
        {
            var cnt = 0;
            var list = frame.ResolveList(boxCombatant->entitiesHit);
            foreach (var eh in list)
            {
                if (eh.hitByHitboxIdentifier == hitboxID) cnt++;
            }
            return cnt;
        }
        
        public static void MarkEntityAsTouched(Frame frame, BoxCombatant* boxCombatant, EntityRef entityHit, int hitboxID)
        {
            var list = frame.ResolveList(boxCombatant->entitiesHit);
            list.Add(new EntityHitDefinition(){ ent = entityHit, hitByHitboxIdentifier = hitboxID});
        }
#endregion

#region Specific Hit Check

        public static void ClearEntityHitTypeDictionary(Frame frame, BoxCombatant* boxCombatant)
        {
            var hbcDictionary = frame.ResolveDictionary(boxCombatant->hitReactionCounters);
            hbcDictionary.Clear();
        }

        public static int GetEntitiesHitCount(Frame frame, BoxCombatant* boxCombatant)
        {
            var hbcDictionary = frame.ResolveDictionary(boxCombatant->hitReactionCounters);
            return hbcDictionary.ContainsKey((int)StandardHitReactions.Hit) ? hbcDictionary[(int)StandardHitReactions.Hit] : 0;
        }
        
        public static int GetEntitiesBlockedCount(Frame frame, BoxCombatant* boxCombatant)
        {
            var hbcDictionary = frame.ResolveDictionary(boxCombatant->hitReactionCounters);
            return hbcDictionary.ContainsKey((int)StandardHitReactions.Blocked) ? hbcDictionary[(int)StandardHitReactions.Blocked] : 0;
        }
        
        public static int GetEntitiesHitOrBlockedCount(Frame frame, BoxCombatant* boxCombatant)
        {
            var hbcDictionary = frame.ResolveDictionary(boxCombatant->hitReactionCounters);
            var hitCount = hbcDictionary.ContainsKey((int)StandardHitReactions.Hit)
                ? hbcDictionary[(int)StandardHitReactions.Hit]
                : 0;
            var blockCount = hbcDictionary.ContainsKey((int)StandardHitReactions.Blocked)
                ? hbcDictionary[(int)StandardHitReactions.Blocked]
                : 0;
            return hitCount + blockCount;
        }
#endregion
        
        public static void CleanupAllBoxes(Frame frame, BoxCombatant* boxCombatant)
        {
            HitboxHelper.CleanupHitboxes(frame, boxCombatant);
            HurtboxHelper.CleanupHurtboxes(frame, boxCombatant);
            CollisionboxHelper.CleanupCollisionboxes(frame, boxCombatant);
            ThrowboxHelper.CleanupThrowboxes(frame, boxCombatant);
        }
    }
}