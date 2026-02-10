using System;
using Quantum;

namespace HnSF.core.state.decisions
{
    [Serializable]
    public unsafe partial class CheckGotHitReactionCount : HNSFStateDecision
    {
        public int minimumHitCount = 1;
        public StandardHitReactions[] hitCheckTypes = new[] { StandardHitReactions.Hit };
    
        public override bool Decide(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<BoxCombatant>(entity, out var boxCombatant)) return false;

            var hCountDictionary = frame.ResolveDictionary(boxCombatant->hitReactionCounters);
            
            int cnt = 0;
            foreach (var hct in hitCheckTypes)
            {
                switch (hct)
                {
                    case StandardHitReactions.Missed:
                        if (hCountDictionary.TryGetValue((int)StandardHitReactions.Missed, out var missedCount))
                            cnt += missedCount;
                        break;
                    case StandardHitReactions.Hit:
                        if (hCountDictionary.TryGetValue((int)StandardHitReactions.Hit, out var hitCount))
                            cnt += hitCount;
                        break;
                    case StandardHitReactions.Blocked:
                        if (hCountDictionary.TryGetValue((int)StandardHitReactions.Blocked, out var blockCount))
                            cnt += blockCount;
                        break;
                    case StandardHitReactions.Perfect_Guard:
                        if (hCountDictionary.TryGetValue((int)StandardHitReactions.Perfect_Guard, out var perfectGuardCount))
                            cnt += perfectGuardCount;
                        break;
                    case StandardHitReactions.Parried:
                        if (hCountDictionary.TryGetValue((int)StandardHitReactions.Parried, out var parriedCount))
                            cnt += parriedCount;
                        break;
                }
            }
            return cnt >= minimumHitCount;
        }

        public override HNSFStateDecision Copy()
        {
            return CopyTo(new CheckGotHitReactionCount());
        }

        public override HNSFStateDecision CopyTo(HNSFStateDecision target)
        {
            var t = target as CheckGotHitReactionCount;
            t.minimumHitCount = minimumHitCount;
            t.hitCheckTypes = new StandardHitReactions[hitCheckTypes.Length];
            Array.Copy(hitCheckTypes, t.hitCheckTypes, hitCheckTypes.Length);
            return base.CopyTo(target);
        }
    }
}