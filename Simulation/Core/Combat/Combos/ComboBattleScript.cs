using System;
using HnSF.core.GroupControl.Combo.Considerations;
using Photon.Deterministic;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF.core.GroupControl.Combo
{
    public unsafe partial class ComboBattleScript : BattleActorGroupControlScript
    {
#if QUANTUM_UNITY
        [Header("Combo Info")]
#endif
        public FP baseWeight = 1;
        public FP idealRange;
        public int cooldownFrames;
        public int minimumRepeatGap;
        public FP repeatChanceMultiplier = FP._0_50;
        
        public int minimumNextAttackDelayFrames;
        public int maximumNextAttackDelayFrames;
        
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public AIComboConsideration[] considerations = Array.Empty<AIComboConsideration>();
        
        public virtual bool CanSelect(Frame frame, EntityRef aiEntity, ref BattleScriptContext context, ref ComboDecisionContext comboContext)
        {
            return RulesValid(frame, aiEntity, ref context);
        }

        public virtual FP CalculateWeight(Frame frame, EntityRef aiEntity, ref BattleScriptContext context, ref ComboDecisionContext comboContext)
        {
            if (!CanSelect(frame, aiEntity, ref context, ref comboContext))
                return FP._0;

            if (!IsOffCooldown(frame, aiEntity, ref context, ref comboContext))
                return FP._0;

            if (!RepeatGapSatisfied(frame, aiEntity, ref context, ref comboContext))
                return FP._0;
            
            FP score = baseWeight;

            score *= ApplyRepeatChanceModifier(frame, aiEntity, ref context, ref comboContext);

            if (score <= FP._0)
                return FP._0;
            
            foreach (var consideration in considerations)
            {
                score *= FPMath.Clamp01(consideration.Calculate(frame, aiEntity, ref context, ref comboContext));

                if (score <= FP._0)
                    break;
            }

            return FPMath.Max(FP._0, score);
        }

        private FP ApplyRepeatChanceModifier(Frame frame, EntityRef aiEntity, ref BattleScriptContext context, ref ComboDecisionContext comboContext)
        {
            FP multi = FP._1;
            
            var comboHistory = frame.ResolveList(comboContext.comboMemory->history);
            
            for (int i = comboHistory.Count - 1; i >= 0; i--)
            {
                if (comboHistory[i] == comboContext.comboIndex)
                    multi *= repeatChanceMultiplier;
            }
            
            return multi;
        }

        private bool RepeatGapSatisfied(Frame frame, EntityRef aiEntity, ref BattleScriptContext context, ref ComboDecisionContext comboContext)
        {
            if (minimumRepeatGap <= 0) return true;
            var comboHistory = frame.ResolveList(comboContext.comboMemory->history);

            int gap = 0;
            
            for (int i = comboHistory.Count - 1; i >= 0; i--)
            {
                if (comboHistory[i] == comboContext.comboIndex)
                    break;
                gap++;

                if (gap >= minimumRepeatGap)
                    return true;
            }
            
            return gap >= minimumRepeatGap;
        }

        private bool IsOffCooldown(Frame frame, EntityRef aiEntity, ref BattleScriptContext context, ref ComboDecisionContext comboContext)
        {
            var cooldownDict = frame.ResolveDictionary(comboContext.comboMemory->perMoveCooldowns);

            if (!cooldownDict.ContainsKey(comboContext.comboIndex))
                return true;

            if (frame.Number < cooldownDict[comboContext.comboIndex]) return false;
            
            cooldownDict.Remove(comboContext.comboIndex);
            return true;

        }
    }
}
