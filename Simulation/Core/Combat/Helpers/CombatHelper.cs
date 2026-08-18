using System;
using System.Collections.Generic;
using HnSF.core.state;
using HnSF.core.state.actions;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core
{
    public static unsafe partial class CombatHelper
    {
        private static LayerMask hitboxMask;

        public static List<Type> CombatBoxesStateActionTypes = new List<Type>()
        {
        };
        
        public static bool WillBeHitThisFrame(Frame f, EntityRef entity)
        {
            return false;
        }
        
        public static bool WillBeHitThisFrame(Frame f, EntityRef entity, out FPVector3 originOfAttack)
        {
            originOfAttack = FPVector3.Zero;
            return false;
        }
        
        
        public static bool WillBeHitThisFrame(Frame f, EntityRef entity, out EntityRef originEntityRef, bool returnHitboxIfNoOwner = false, bool failIfNoEntity = false)
        {
            originEntityRef = EntityRef.None;
            return false;
        }

        public static void RunActionsOnEntity(Frame f, EntityRef entityRef, HNSFStateAction[] actions)
        {
            if (f.Unsafe.TryGetPointer<GenericStateMachine>(entityRef, out var genericStateAgent))
            {
                if (!f.TryFindAsset<AIConfig>(genericStateAgent->config.Id, out var attackerAiConfig)) return;
                HNSFStateContext genericContext = new HNSFStateContext(
                    &genericStateAgent->stateAgent.stateData,
                    &genericStateAgent->blackboard,
                    attackerAiConfig,
                    genericStateAgent->stateAgent.stateData.frame);
                
                for (int i = 0; i < actions.Length; i++)
                {
                    if (actions[i] == null) continue;
                    
                    bool conditionsValid = true;
                    foreach (var deci in actions[i].decision)
                    {
                        if (deci.Decide(f, entityRef, ref genericContext)) continue;
                        conditionsValid = false;
                        break;
                    }
                    if (conditionsValid == false) continue;
                    
                    actions[i].ExecuteAction(f, entityRef, 0, ref genericContext);
                }
            }
        }
        
        public static void RunActionsOnEntity(Frame f, EntityRef entityRef, List<HNSFStateAction> actions)
        { 
            if (f.Unsafe.TryGetPointer<GenericStateMachine>(entityRef, out var genericStateAgent))
            {
                if (!f.TryFindAsset<AIConfig>(genericStateAgent->config.Id, out var attackerAiConfig)) return;
                HNSFStateContext genericContext = new HNSFStateContext(
                    &genericStateAgent->stateAgent.stateData,
                    &genericStateAgent->blackboard,
                    attackerAiConfig,
                    genericStateAgent->stateAgent.stateData.frame);
                
                for (int i = 0; i < actions.Count; i++)
                {
                    if (actions[i] == null) continue;
                    
                    bool conditionsValid = true;
                    foreach (var deci in actions[i].decision)
                    {
                        if (deci.Decide(f, entityRef, ref genericContext)) continue;
                        conditionsValid = false;
                        break;
                    }
                    if (conditionsValid == false) continue;
                    
                    actions[i].ExecuteAction(f, entityRef, 0, ref genericContext);
                }
            }
        }
    }
}
