using System;
using System.Collections.Generic;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.GroupControl.Combo
{
    public static unsafe partial class AIComboSelector
    {
        private static List<ComboCandidate> comboCandidates = new();
        
        private static List<ComboCandidate> readyComboCandidates = new();
        private static List<ComboCandidate> consideredComboCandidates = new();
        
        public static (int, bool) SelectCombo(Frame frame, EntityRef agentEntityRef, ref BattleScriptContext context, 
            AssetRef<ComboBattleScript>[] scriptList, Func<Frame, FP, List<ComboCandidate>, bool, (int, bool)> SelectorAction)
        {
            comboCandidates.Clear();
            
            readyComboCandidates.Clear();
            consideredComboCandidates.Clear();

            FP readyTotalWeight = FP._0;
            FP considerTotalWeight = FP._0;
            
            ComboDecisionContext cdc = new ComboDecisionContext();
            frame.AddOrGet(agentEntityRef, out cdc.comboMemory);
            
            for (int i = 0; i < scriptList.Length; i++)
            {
                var comboAssetRef =  scriptList[i];
                
                if(!frame.TryFindAsset(comboAssetRef, out var comboScript))
                    continue;
                
                cdc.currentlyEvaluating = comboScript;
                cdc.comboIndex = (byte)i;
                
                if(!comboScript.CanSelectAsPotentialCombo(frame, agentEntityRef, ref context, ref cdc))
                    continue;

                var weight = comboScript.CalculateWeight(frame, agentEntityRef, ref context, ref cdc);

                if (weight <= FP._0)
                    continue;

                var cIndex = i;
                
                if (comboScript.CanExecuteNow(frame, agentEntityRef, ref context, ref cdc))
                {
                    readyTotalWeight += weight;
                    
                    readyComboCandidates.Add(new ComboCandidate()
                    {
                        scriptIndex = cIndex,
                        comboScript = comboScript,
                        weight = weight
                    });
                }
                else if(readyComboCandidates.Count == 0)
                {
                    considerTotalWeight += weight;
                    
                    consideredComboCandidates.Add(new ComboCandidate()
                    {
                        scriptIndex = cIndex,
                        comboScript = comboScript,
                        weight = weight
                    });
                }
            }

            if (readyComboCandidates.Count > 0)
            {
                return SelectorAction.Invoke(frame, readyTotalWeight, readyComboCandidates, false);
            }else if (consideredComboCandidates.Count > 0)
            {
                return SelectorAction.Invoke(frame, considerTotalWeight, consideredComboCandidates, true);
            }

            return (-1, false);
        }
        
        public static unsafe (int, bool) SelectWeighted(Frame frame, FP totalWeight,
            List<ComboCandidate> candidates, bool consideration)
        {
            if (candidates.Count <= 0 || totalWeight <= 0)
                return (-1, consideration);

            if (candidates.Count == 1)
                return (candidates[0].scriptIndex, consideration);
            
            FP roll = frame.RNG->Next(0, totalWeight);

            FP accumulatedWeight = 0;

            for (int i = 0; i < candidates.Count; ++i)
            {
                FP weight = candidates[i].weight;

                if (weight <= 0)
                    continue;

                accumulatedWeight += weight;

                if (roll <= accumulatedWeight)
                    return (candidates[i].scriptIndex, consideration);
            }
            
            return (-1, consideration);
        }
    }
}