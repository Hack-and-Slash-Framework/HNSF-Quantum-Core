using System;
using System.Collections.Generic;
using Quantum;

namespace HnSF.core.state
{
    public static unsafe partial class HNSFStateHelper
    {
        public static unsafe partial class Generic
        {
            public static void UpdateState(Frame frame, HNSFStateAgentData* agentData, EntityRef entity, 
                AIBlackboardComponent* blackboard, AIConfigBase aiConfig)
            {
                if (!frame.TryFindAsset<HNSFState>(agentData->state.Id, out var currentState)) return;
                
                HNSFStateContext stateContext = new HNSFStateContext(
                    agentData,
                    blackboard,
                    aiConfig,
                    agentData->frame);
                HNSFStateHelper.Update(frame, frame.DeltaTime, entity, ref stateContext);
            
                if (!agentData->dontAutoIncrementFrame && currentState.autoIncrement) agentData->frame++;
                if (agentData->frame > currentState.totalFrames)
                {
                    if (currentState.autoLoop)
                    {
                        agentData->frame = currentState.autoLoopFrame;
                    }
                    else
                    {
                        agentData->frame = currentState.totalFrames;
                    }
                }
                agentData->dontAutoIncrementFrame = false;
                agentData->realFrame++;
            }

            public static void UpdateGenericStateMachine(Frame frame, EntityRef entity, bool checkForTransitions = true)
            {
                if (!frame.Unsafe.TryGetPointer<GenericStateMachine>(entity, out var genericStateMachine)
                    || !frame.TryFindAsset(genericStateMachine->config, out var config)) return;
                UpdateGenericStateMachine(frame, entity, genericStateMachine, config, checkForTransitions);
            }
            
            public static void UpdateGenericStateMachine(Frame frame, EntityRef entity, GenericStateMachine* genericStateAgent,
                AIConfigBase config, bool checkForTransitions = true)
            {
                UpdateState(frame,
                    &genericStateAgent->stateAgent.stateData,
                    entity,
                    &genericStateAgent->blackboard,
                    config);

                if(checkForTransitions) CheckForStateChange(frame, entity, genericStateAgent, config);
            }
            
            public static void CheckForStateChange(Frame frame, EntityRef entity)
            {
                if (!frame.Unsafe.TryGetPointer<GenericStateMachine>(entity, out var gsm)
                    || !frame.TryFindAsset(gsm->config, out var aiConfig)) return;
                CheckForStateChange(frame, entity, gsm, aiConfig);
            }
            
            public static void CheckForStateChange(Frame frame, EntityRef entity, GenericStateMachine* stateAgent,
                AIConfigBase config)
            {
                // GENERAL STATE STUFF
                var wasStateChanged = CheckForStateChange(frame, entity,
                    &stateAgent->stateAgent.stateData,
                    &stateAgent->blackboard,
                    config, true);

                frame.TryFindAsset<HNSFState>(stateAgent->stateAgent.stateData.state.Id,
                    out var currentStateAsset);
                
                if (wasStateChanged
                    && currentStateAsset
                    && currentStateAsset.clearInputBuffer
                    && frame.Unsafe.TryGetPointer<ActorInputBuffer>(entity, out var actorInputInfo))
                {
                    InputHelper.DisableLastInput(frame, actorInputInfo);
                }
            }

            private static bool CheckForStateChange(Frame frame, EntityRef entity, HNSFStateAgentData* stateData, 
                AIBlackboardComponent* blackboard, AIConfigBase config, bool cleanup = false)
            {
                if (stateData->toStateRequested)
                {
                    ChangeState(frame, stateData, entity, blackboard, config, cleanup);
                    return true;
                }
                return false;
            }
        
            private static void ChangeState(Frame frame, HNSFStateAgentData* stateData, 
                EntityRef entity, AIBlackboardComponent* blackboard, AIConfigBase config, bool cleanup = false)
            {
                HNSFStateContext stateContext = new HNSFStateContext(
                    stateData,
                    blackboard,
                    config,
                    stateData->frame);
                CauseStateTransition(frame, frame.DeltaTime, entity, ref stateContext, cleanup);
            }
            
            public static void ChangeState(Frame frame, EntityRef entityRef, AssetRef<HNSFState> stateRef, bool immediateTransition = false)
            {
                if (!frame.Unsafe.TryGetPointer<GenericStateMachine>(entityRef, out var csm)
                    || !frame.TryFindAsset(csm->config, out var config)) return;
                
                csm->stateAgent.stateData.toStateRequested = true;
                csm->stateAgent.stateData.toState = stateRef;
                csm->stateAgent.stateData.toFrame = 0;
                
                if (immediateTransition)
                {
                    CheckForStateChange(frame, entityRef, csm, config);
                }
            }
            
            public static void ChangeStateByTag(Frame frame, EntityRef entityRef, AssetRef<Tag> stateTag, bool immediateTransition = false)
            {
                if (!frame.Unsafe.TryGetPointer<GenericStateMachine>(entityRef, out var csm)
                    || !frame.TryFindAsset(csm->config, out var config)) return;
                if(!frame.TryFindAsset(csm->stateAgent.stateSet, out var stateSet)) return;
                if (!stateSet.AttemptGetStateByTag(csm->stateAgent.stateData.moveset, stateTag, out var toStateRef)) return;
                
                csm->stateAgent.stateData.toStateRequested = true;
                csm->stateAgent.stateData.toState = toStateRef;
                csm->stateAgent.stateData.toFrame = 0;
                
                if (immediateTransition)
                {
                    CheckForStateChange(frame, entityRef, csm, config);
                }
            }
            
            public static bool ChangeStateByTag(Frame frame, EntityRef entityRef, GenericStateMachine* csm, AIConfigBase config, AssetRef<Tag> stateTag, bool immediateTransition = false)
            {
                if(!frame.TryFindAsset(csm->stateAgent.stateSet, out var stateSet)) return false;
                if (!stateSet.AttemptGetStateByTag(csm->stateAgent.stateData.moveset, stateTag, out var toStateRef)) return false;

                csm->stateAgent.stateData.toStateRequested = true;
                csm->stateAgent.stateData.toState = toStateRef;
                csm->stateAgent.stateData.toFrame = 0;
                
                if (immediateTransition)
                {
                    CheckForStateChange(frame, entityRef, csm, config);
                }
                return true;
            }
        }
    }
}