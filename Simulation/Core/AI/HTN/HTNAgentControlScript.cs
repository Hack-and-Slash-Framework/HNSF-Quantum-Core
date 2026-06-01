using System;
using System.Collections.Generic;
using HnSF.core.GroupControl;
using HnSF.core.GroupControl.Actions;
using HnSF.core.GroupControl.Grabbers;
using Photon.Deterministic;
#if QUANTUM_UNITY
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
#endif

namespace Quantum
{
#if QUANTUM_UNITY
    [MovedFrom(autoUpdateAPI: false, sourceClassName: "CWAControlScript")]
#endif
    public unsafe partial class HTNAgentControlScript : BattleActorGroupControlScript
    {
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public GroupControlRule[] tickConditionals = Array.Empty<GroupControlRule>();
        
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public List<GroupControlAction> actionsOnExit = new List<GroupControlAction>();
        
        public List<HTNBehaviourDefinition.BehaviourSet> behaviourSets;
        
        public bool uninterruptible;
        public FP minCooldown;
        public FP maxCooldown;
        
        
        private void OnValidate()
        {
            Bake();
        }

        public bool AreTickConditionalsValid(Frame frame, EntityRef infoEntityRef)
        {
            if(tickConditionals.Length == 0) return true;
            foreach (var rule in tickConditionals)
            {
                if(!rule.IsValid(frame, infoEntityRef)) return false;
            }
            return true;
        }

        public unsafe void ExecuteOnExitActions(Frame frame, EntityRef infoEntityRef)
        {
            var groupControlContext = new GroupControlContext();
            groupControlContext.SetScriptEntityAndBlackboard(frame, infoEntityRef, null);
            
            for (int i = 0; i < actionsOnExit.Count; i++)
            {
                actionsOnExit[i].OnEnter(frame, infoEntityRef, ref groupControlContext);
                actionsOnExit[i].Tick(frame, infoEntityRef, ref groupControlContext);
                actionsOnExit[i].OnExit(frame, infoEntityRef, ref groupControlContext);
            }
        }

        public unsafe bool CheckBehaviours(Frame frame, EntityRef agentEntityRef, HTNAgent* agent, BattleActorAI* actorAI)
        {
            agent->currentActionData.currentAction = -1;
            agent->currentActionData.script = default;
            
            foreach (var behaviourSet in behaviourSets)
            {
                if(!behaviourSet.DoRulesPass(frame, agentEntityRef)) continue;
                var nextActionIndex = behaviourSet.actionsWeighted.Next(frame.RNG);

                if (nextActionIndex < 0) return false;
                if (!frame.TryFindAsset(behaviourSet.actions[nextActionIndex].action, out var behaviour)) return false;

                agent->cooldown = frame.RNG->NextInclusive(behaviour.minCooldown, behaviour.maxCooldown);
                agent->currentActionData.script = behaviourSet.actions[nextActionIndex].action.Id;
                agent->currentActionData.currentAction = 0;
                agent->uninterruptible = behaviour.uninterruptible;
                var groupControlContext = new GroupControlContext();
                groupControlContext.SetScriptEntityAndBlackboard(frame, agentEntityRef, null);
                agent->currentActionData.Initialize(frame, agentEntityRef, ref groupControlContext);
                return true;
            }
            return false;
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
        }
    }
}
