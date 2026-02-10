using System;
using Quantum;

namespace HnSF.core.state.functions
{
    [Serializable]
    public unsafe partial class GetHitReactionCount : StateFunctionInt
    {
        public StandardHitReactions[] hitCheckTypes = new[] { StandardHitReactions.Hit };
        
        public override int Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<BoxCombatant>(entity, out var boxCombatant)) return 0;

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
            return cnt;
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new GetHitReactionCount());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as GetHitReactionCount;
            t.hitCheckTypes = new StandardHitReactions[hitCheckTypes.Length];
            Array.Copy(hitCheckTypes, t.hitCheckTypes, hitCheckTypes.Length);
            return base.CopyTo(target);
        }
    }
}