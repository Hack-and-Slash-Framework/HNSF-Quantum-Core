using System;
using System.Collections.Generic;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state
{
    public static unsafe partial class HNSFStateHelper
    {
        public static void CauseStateTransition(Frame frame, FP deltaTime, EntityRef entity, ref HNSFStateContext stateContext, bool cleanup = false)
        {
            if (!frame.TryFindAsset(stateContext.agentData->state, out HNSFState currentState))
            {
                Log.Error($"Could not find Current State while forcing a transition.");
                return;
            }
            CauseStateTransition(frame, deltaTime, entity, currentState, ref stateContext, cleanup);
        }
    
        public static void CauseStateTransition(Frame frame, FP deltaTime, EntityRef entity, HNSFState currentState, ref HNSFStateContext stateContext, bool cleanup = false)
        {
            var stateData = stateContext.agentData;
            
            EventReceiverHelper.CallEvent(frame, entity, (int)EventReceiverTyping.StateChanged);
            
            // On Interrupted
            stateData->frame = currentState.totalFrames+1;
            stateContext.stateFrame = currentState.totalFrames+1;
            UpdateState(frame, entity, ref stateContext);

            var hasToStateAsset = frame.TryFindAsset(stateContext.agentData->toState, out var toStateAsset);
            
            if (cleanup && frame.Unsafe.TryGetPointer<BoxCombatant>(entity, out var bc) && hasToStateAsset)
            {
                BoxCombatantHelper.CleanupAllBoxes(frame, bc);
                if (!toStateAsset.dontClearHitEntities)
                {
                    BoxCombatantHelper.ClearTouchedEntities(frame, bc);
                    BoxCombatantHelper.ClearEntityHitTypeDictionary(frame, bc);
                }
            }
        
            stateData->toStateRequested = false;

            EventReceiverHelper.Unregister(frame, entity, stateData->state.Id.Value);
            
            // On Start
            stateData->state = stateData->toState;
            stateContext.workingState = stateData->state;
            stateContext.agentData->frame = 0;
            stateContext.agentData->realFrame = 0;
            stateContext.stateFrame = 0;

            if (!hasToStateAsset || toStateAsset.incrementStateCounter)
            {
                stateData->uniqueStateId = ( (stateData->uniqueStateId + 1) % (uint.MaxValue-1));
            }
            
            UpdateState(frame, entity, ref stateContext);
            stateContext.agentData->frame = stateData->toFrame == 0 ? 1 : stateData->toFrame;
            stateContext.stateFrame = stateContext.agentData->frame;
            stateContext.realStateFrame = stateContext.stateFrame;
        }
    
        public static void UpdateState(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            Update(frame, frame.DeltaTime, entity, ref stateContext);
        }
    
        public static void Update(Frame frame, FP deltaTime, EntityRef entity, ref HNSFStateContext context)
        {
            ThreadSafe.Update((FrameThreadSafe)frame, deltaTime, context.agentData, entity, ref context);
        }
    
        public static void Update(Frame frame, FP deltaTime, EntityRef entity, HNSFState state, ref HNSFStateContext context)
        {
            ThreadSafe.Update((FrameThreadSafe)frame, deltaTime, context.agentData, entity, state, ref context);
        }

        public static bool IsFrameWithinRanges(this ActionRange[] actionRangeList, int stateTotalFrames, int frame)
        {
            if (actionRangeList == null || actionRangeList.Length == 0) return false;
            foreach (var actionRange in actionRangeList)
            {
                var start = StateFrameHelper.ConvertFrame(stateTotalFrames, actionRange.start);
                var end = StateFrameHelper.ConvertFrame(stateTotalFrames, actionRange.end);
                if (end < start) end = start;
                if (frame < start || frame > end) continue;
                return true;
            }
            return false;
        }
    
        public static bool IsFrameWithinRanges(this ActionRange[] actionRangeList, int stateTotalFrames, int frame, out FP percentage)
        {
            percentage = 1;
            if (actionRangeList == null || actionRangeList.Length == 0) return false;
            foreach (var actionRange in actionRangeList)
            {
                var start = StateFrameHelper.ConvertFrame(stateTotalFrames, actionRange.start);
                var end = StateFrameHelper.ConvertFrame(stateTotalFrames, actionRange.end);
                if (end < start) end = start;
                if (frame < start || frame > end) continue;
                percentage = frame == end ? 1 : (FP)(frame-start) / (FP)(end-start);
                return true;
            }
            return false;
        }

        public static void ChangeMoveset(Frame frame, EntityRef entityRef, AssetRef<Tag> toMoveset)
        {
            if (!frame.Unsafe.TryGetPointer<GenericStateMachine>(entityRef, out var gsm)) return;
            EventReceiverHelper.Unregister(frame, entityRef, gsm->stateAgent.stateData.moveset.Id.Value);
            gsm->stateAgent.stateData.moveset = toMoveset;
        }
        
        public static EntityRef GetStateTargetEntity(Frame frame, ref StateActionTargetContext targetContext)
        {
            switch (targetContext.targetType)
            {
                case StateActionTargetType.Self:
                    return targetContext.callingEntity;
                case StateActionTargetType.Throwee:
                    if(!frame.Unsafe.TryGetPointer<IsThrowing>(targetContext.callingEntity, out var isThrowing)) return EntityRef.None;
                    var throwingDict = frame.ResolveDictionary(isThrowing->throwees);
                    return throwingDict.TryGetValue(targetContext.throweeId, out var throweeEntityRef) ? throweeEntityRef : EntityRef.None;
                case StateActionTargetType.ArticleOwner:
                    if(!frame.Unsafe.TryGetPointer<Article>(targetContext.callingEntity, out var article)) return EntityRef.None;
                    return article->owner;
                case StateActionTargetType.ArticleOwnerRoot:
                    var rootOwner = Article.GetRootOwner(frame, targetContext.callingEntity);
                    return rootOwner;
                case StateActionTargetType.LastCreatedArticle:
                    if(!frame.Unsafe.TryGetPointer<ArticlesOwner>(targetContext.callingEntity, out var articlesOwner)) return EntityRef.None;
                    var articlesList = frame.ResolveList(articlesOwner->articleRefs);
                    if(articlesList.Count == 0) return EntityRef.None;
                    return articlesList[^1];
                case StateActionTargetType.FromEntityMap:
                    if(!frame.Unsafe.TryGetPointer<TaggedEntityMapping>(targetContext.callingEntity, out var taggedEntityMapping)) return EntityRef.None;
                    var mappingDict = frame.ResolveDictionary(taggedEntityMapping->tagToEntityMap);
                    return mappingDict.TryGetValue(targetContext.mapTag, out var mapEntityRef) ? mapEntityRef : EntityRef.None;
                case StateActionTargetType.LastHitEntity:
                    if(!frame.Unsafe.TryGetPointer<LastHitWithInfo>(targetContext.callingEntity, out var lastHitWithInfo)) return EntityRef.None;
                    if (lastHitWithInfo->data.Field == Quantum.LastHitWithData.HITINFODATA)
                    {
                        return lastHitWithInfo->data.hitInfoData->lastHitEntity;
                    }else if (lastHitWithInfo->data.Field == LastHitWithData.THROWINFODATA)
                    {
                        // TODO
                        return EntityRef.None;
                    }
                    return EntityRef.None;
                case StateActionTargetType.FromFunction:
                    var tempContext = new HNSFStateContext(frame, targetContext.callingEntity);
                    return targetContext.entityRefFunction.Execute(frame, targetContext.callingEntity, ref tempContext);
                    break;
            }
            return EntityRef.None;
        }
        
        public static AssetRef<HNSFState> GetEntityState(Frame frame, EntityRef attackerEntityRef)
        {
            if (frame.Unsafe.TryGetPointer<GenericStateMachine>(attackerEntityRef, out var acStateMachines))
            {
                return acStateMachines->stateAgent.stateData.state;
            }
            return default;
        }
        
        public static uint GetEntityStateId(Frame frame, EntityRef attackerEntityRef)
        {
            if (frame.Unsafe.TryGetPointer<GenericStateMachine>(attackerEntityRef, out var acStateMachines))
            {
                return acStateMachines->stateAgent.stateData.uniqueStateId;
            }
            return 0;
        }
    }
}