using System;
using System.Collections.Generic;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.GroupControl.Combo
{
    public static unsafe partial class AIComboSelector
    {
        private static List<ComboCandidate> comboCandidates = new();
        public static int SelectCombo(Frame frame, EntityRef entityRef, ref BattleScriptContext context, 
            AssetRef<ComboBattleScript>[] scriptList, Func<Frame, FP, List<ComboCandidate>, int> SelectorAction)
        {
            comboCandidates.Clear();

            FP totalWeight = FP._0;

            ComboDecisionContext cdc = new ComboDecisionContext();
            frame.AddOrGet(entityRef, out cdc.comboMemory);
            
            for (int i = 0; i < scriptList.Length; i++)
            {
                var comboAssetRef =  scriptList[i];
                
                if(!frame.TryFindAsset(comboAssetRef, out var comboScript))
                    continue;
                
                cdc.currentlyEvaluating = comboScript;
                cdc.comboIndex = (byte)i;
                
                if(!comboScript.CanSelect(frame, entityRef, ref context, ref cdc))
                    continue;
                
                var weight = comboScript.CalculateWeight(frame, entityRef, ref context, ref cdc);

                if (weight <= FP._0)
                    continue;
                
                comboCandidates.Add(new ComboCandidate()
                {
                    scriptIndex = i,
                    comboScript = comboScript,
                    weight = weight
                });

                totalWeight += weight;
            }
            
            return comboCandidates.Count == 0 ? -1 : SelectorAction.Invoke(frame, totalWeight, comboCandidates);
        }
        
        public static unsafe int SelectWeighted(Frame frame, FP totalWeight,
            List<ComboCandidate> candidates)
        {
            if (candidates.Count <= 0 || totalWeight <= 0)
                return -1;
            
            FP roll = frame.RNG->Next(0, totalWeight);

            FP accumulatedWeight = 0;

            for (int i = 0; i < candidates.Count; ++i)
            {
                FP weight = candidates[i].weight;

                if (weight <= 0)
                    continue;

                accumulatedWeight += weight;

                if (roll < accumulatedWeight)
                    return candidates[i].scriptIndex;
            }
            
            return -1;
        }
    }
}