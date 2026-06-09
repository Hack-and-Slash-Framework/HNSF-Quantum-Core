using System;
using System.Collections.Generic;
using HnSF;
using HnSF.core.GroupControl;
using HnSF.core.GroupControl.Actions;
using HnSF.core.GroupControl.Grabbers;
using Photon.Deterministic;
using UnityEngine.Scripting.APIUpdating;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    [MovedFrom(autoUpdateAPI: false, sourceClassName: "CWABehaviourDefinition")]
    public class HTNBehaviourDefinition : AssetObject
    {
        [Serializable]
        public struct ActionScriptWithWeight
        {
            public int weight;
            public AssetRef<HTNAgentControlScript> action;
        }
        
        [Serializable]
        public class BehaviourSet
        {
#if QUANTUM_UNITY
            [SerializeReference, SubclassSelector]
#endif
            public GroupControlRule[] rules;
            public List<ActionScriptWithWeight> actions;
            public WeightedList<int> actionsWeighted;

            public bool DoRulesPass(Frame frame, EntityRef agentEntityRef)
            {
                foreach (var rule in rules)
                {
                    if (!rule.IsValid(frame, agentEntityRef)) return false;
                }
                return true;
            }
            
            public void Bake()
            {
                if (actions == null)
                {
                    actionsWeighted = new WeightedList<int>();
                    return;
                }
                
                List<WeightedListItem<int>> items = new();
                for (var index = 0; index < actions.Count; index++)
                {
                    var action = actions[index];
                    items.Add(new WeightedListItem<int>(index, action.weight));
                }
                actionsWeighted = new WeightedList<int>(items);
            }
        }
        
        public FP minBehaviourCooldown;
        public List<BehaviourSet> behaviourSets;

        public unsafe void Tick(Frame frame, EntityRef agentEntityRef, HTNAgent* agent, BattleActorAI* actorAI)
        {
            var groupControlContext = new BattleScriptContext();
            groupControlContext.SetScriptEntityAndBlackboard(frame, agentEntityRef, null);

            var htnContext = new HTNAgentContext(agent, actorAI);
            groupControlContext.SetUserData(1, &htnContext);
            
            if (agent->currentActionData.script != default)
            {
                var foundScript = frame.TryFindAsset(agent->currentActionData.script, out var controlScript);

                if (agent->currentActionResult == HTNTaskResult.FAILURE)
                {
                    if(foundScript)
                        (controlScript as HTNAgentControlScript).ExecuteOnExitActions(frame, agentEntityRef);
                    agent->ClearCurrentScript();
                    agent->ResetCooldown();
                } else if (foundScript && (controlScript as HTNAgentControlScript).AreTickConditionalsValid(frame, agentEntityRef) == false)
                {
                    (controlScript as HTNAgentControlScript).ExecuteOnExitActions(frame, agentEntityRef);
                    agent->ClearCurrentScript();
                    agent->ResetCooldown();
                }
                else
                {
                    if (agent->currentActionData.Tick(frame, agentEntityRef, ref groupControlContext))
                    {
                        if (agent->currentActionData.IsEnd(frame, ref groupControlContext))
                        {
                            if (foundScript)
                            {
                                var cwaScript = (controlScript as HTNAgentControlScript);

                                if (cwaScript.CheckBehaviours(frame, agentEntityRef, agent, actorAI))
                                {
                                    cwaScript.ExecuteOnExitActions(frame, agentEntityRef);
                                    agent->currentActionData.Initialize(frame, agentEntityRef, ref groupControlContext);
                                }else {
                                    agent->currentActionData.script = default;
                                    agent->currentActionData.currentAction = -1;
                                }
                            }
                            else
                            {
                                agent->ClearCurrentScript();
                            }
                        }
                    }
                }
            }
            
            if (agent->cooldown > 0)
            {
                agent->cooldown -= frame.DeltaTime;
                return;
            }
            agent->cooldown = 0;

            if (agent->uninterruptible)
            {
                return;
            }

            // Splicing.
            if (actorAI->updateInterval > 0)
            {
                var frameSlice = frame.Number % actorAI->updateInterval;
                var entitySlice = agentEntityRef.Index % actorAI->updateInterval;
                if (frameSlice != entitySlice) return;
            }

            bool checkedBehaviours = false;
            if (frame.TryFindAsset(agent->currentActionData.script, out var cs))
            {
                var cwaScript = (cs as HTNAgentControlScript);

                if (agent->currentActionResult == HTNTaskResult.PROCESSING && cwaScript.behaviourSets.Count > 0) checkedBehaviours = true;
                
                if (cwaScript.CheckBehaviours(frame, agentEntityRef, agent, actorAI))
                {
                    cwaScript.ExecuteOnExitActions(frame, agentEntityRef);
                    agent->currentActionData.Initialize(frame, agentEntityRef, ref groupControlContext);
                    return;
                }
            }

            if (checkedBehaviours) return;
            
            foreach (var behaviourSet in behaviourSets)
            {
                if (!behaviourSet.DoRulesPass(frame, agentEntityRef)) continue;
                var nextActionIndex = behaviourSet.actionsWeighted.Next(frame.RNG);

                if (nextActionIndex < 0) return; // exit or continue?
                if (!frame.TryFindAsset(behaviourSet.actions[nextActionIndex].action, out var behaviour)) return;

                agent->cooldown = frame.RNG->NextInclusive(behaviour.minCooldown, behaviour.maxCooldown);
                agent->currentActionData.script = behaviourSet.actions[nextActionIndex].action.Id;
                agent->currentActionData.currentAction = 0;
                agent->uninterruptible = behaviour.uninterruptible;
                agent->currentActionData.Initialize(frame, agentEntityRef, ref groupControlContext);
                break;
            }
        }

        private void OnValidate()
        {
            Bake();
        }

#if QUANTUM_UNITY
        [ContextMenu("Bake")]
#endif
        public void Bake()
        {
            for (int i = 0; i < behaviourSets.Count; i++)
            {
                var temp = behaviourSets[i];
                temp.Bake();
                behaviourSets[i] = temp;
            }
            
            //Debug.Log("Baked " + behaviourSets.Count + " behaviours");
        }
    }
}
