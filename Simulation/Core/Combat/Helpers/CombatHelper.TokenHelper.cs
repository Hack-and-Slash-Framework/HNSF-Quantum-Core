using Quantum;

namespace HnSF.core
{
    public static unsafe partial class CombatHelper
    {
        public static unsafe partial class TokenHelper
        {
            public static void InitializeTokenActor(Frame frame, EntityRef attackingEntityRef)
            {
                var combatTokenSource = frame.Unsafe.GetOrAddSingletonPointer<CombatTokenSource>();
                var tokenActorsDict = frame.ResolveDictionary(combatTokenSource->tokenActorsDict);

                var ctai = new CombatTokenActorInfo()
                {
                    actorEntityRef = attackingEntityRef,
                    maxScore = 100
                };
                ctai.currentTokens = frame.AllocateList<EntityRef>();
                
                tokenActorsDict.Add(attackingEntityRef, ctai);
            }

            public static void UninitalizeTokenActor(Frame frame, EntityRef attackingEntityRef)
            {
                var combatTokenSource = frame.Unsafe.GetOrAddSingletonPointer<CombatTokenSource>();
                var tokenActorsDict = frame.ResolveDictionary(combatTokenSource->tokenActorsDict);

                if (!tokenActorsDict.TryGetValuePointer(attackingEntityRef, out var ctai)) return;
                
                frame.FreeList(ctai->currentTokens);
                ctai->currentTokens = default;
            }

            public static CombatTokenActorInfo* GetCombatTokenActorInfo(Frame frame, EntityRef attackingEntityRef)
            {
                var combatTokenSource = frame.Unsafe.GetOrAddSingletonPointer<CombatTokenSource>();
                var tokenActorsDict = frame.ResolveDictionary(combatTokenSource->tokenActorsDict);

                if (!tokenActorsDict.ContainsKey(attackingEntityRef)) InitializeTokenActor(frame, attackingEntityRef);

                return tokenActorsDict.TryGetValuePointer(attackingEntityRef, out var ctai) ? ctai : default;
            }
            
            public static bool TryGetCombatTokenActorInfo(Frame frame, EntityRef attackingEntityRef, out CombatTokenActorInfo* combatTokenActorInfo)
            {
                var combatTokenSource = frame.Unsafe.GetOrAddSingletonPointer<CombatTokenSource>();
                var tokenActorsDict = frame.ResolveDictionary(combatTokenSource->tokenActorsDict);

                if (!tokenActorsDict.ContainsKey(attackingEntityRef)) InitializeTokenActor(frame, attackingEntityRef);

                return tokenActorsDict.TryGetValuePointer(attackingEntityRef, out combatTokenActorInfo);
            }

            public static bool RequestAttackToken(Frame frame, EntityRef attackerEntityRef, EntityRef attackingEntityRef, byte tokenScore, byte priority, byte tokenType, out EntityRef attackTokenEntityRef)
            {
                attackTokenEntityRef = EntityRef.None;
                if (!TryGetCombatTokenActorInfo(frame, attackingEntityRef, out var combatTokenActorInfo)) return false;
                var currentTokens = frame.ResolveList(combatTokenActorInfo->currentTokens);
                combatTokenActorInfo->CleanupInvalidTokens(frame);
                
                // Check if the attacker already has an attack token for this defender.
                for (int i = currentTokens.Count - 1; i >= 0; i--)
                {
                    if (!frame.Exists(currentTokens[i]))
                    {
                        currentTokens.RemoveAt(i);
                        continue;
                    }
                    if (!frame.Unsafe.TryGetPointer<AttackToken>(currentTokens[i], out var at)) continue;
                    if (at->attackerEntityRef == attackerEntityRef)
                    {
                        attackTokenEntityRef = currentTokens[i];
                        return true;
                    }
                }
                
                // Check if there's room for our token.
                if (tokenScore > combatTokenActorInfo->maxScore
                    || tokenScore > combatTokenActorInfo->maxScore - combatTokenActorInfo->Score(frame)) return false;

                attackTokenEntityRef = frame.Create();
                frame.Add(attackTokenEntityRef, new AttackToken()
                {
                    attackerEntityRef = attackerEntityRef,
                    attackingEntityRef = attackingEntityRef,
                    score = tokenScore,
                    priority = priority,
                    tokenType = 0
                });
                currentTokens.Add(attackTokenEntityRef);
                
                return true;
            }

            public static void ReturnToken(Frame frame, EntityRef attackTokenEntityRef)
            {
                if (!frame.Unsafe.TryGetPointer<AttackToken>(attackTokenEntityRef, out var attackToken)
                    || !frame.Exists(attackToken->attackingEntityRef)
                    || !TryGetCombatTokenActorInfo(frame, attackToken->attackingEntityRef, out var ctai)) return;
                
                ctai->RemoveToken(frame, attackTokenEntityRef);
            }
        }
    }
}
