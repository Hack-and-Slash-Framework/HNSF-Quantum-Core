using System;
using System.Collections.Generic;
using Quantum;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Pool;
using UnityEngine.Profiling;

namespace HnSF
{
    [System.Serializable]
    public unsafe class GlobalCutsceneManager
    {
        protected List<IDisposable> _disposableCallbacks = new List<IDisposable>();

        protected Dictionary<EventKey, EventBattleActorLinkAdded> _unconfirmedLinkCutsceneGrouping = new();
        protected Dictionary<EventKey, EventBattleActorLinkRemoved> _unconfirmedUnlinkCutsceneGrouping = new();

        public QuantumEntityViewUpdater viewUpdater;

        public Dictionary<AssetRef, CutsceneGrouping> cutsceneGroupingPrefabs = new();
        public Dictionary<AssetRef, List<CutsceneGrouping>> cutsceneGroupingPools = new();
        public Dictionary<EntityRef, CutsceneGrouping> entityToCutscenePlayers = new();

        public CutsceneBindingSource globalBindingSource;

        public MatchHandlerBase matchHandler;
        
        public static Dictionary<AssetRef, CutsceneGrouping> singletonCutsceneGroupings = new();

        public virtual void Initialize(MatchHandlerBase mHandler, CutsceneBindingSource bindingSource = null)
        {
            matchHandler = mHandler;
            globalBindingSource = bindingSource ?? new CutsceneBindingSource();
            _disposableCallbacks.Add(
                QuantumCallback.SubscribeManual((CallbackEventCanceled c) => WhenEventCanceled(c)));
            _disposableCallbacks.Add(
                QuantumCallback.SubscribeManual((CallbackEventConfirmed c) => WhenEventConfirmed(c)));
            _disposableCallbacks.Add(QuantumEvent.SubscribeManual((EventBattleActorLinkAdded e) =>
                WhenBattleActorLinkAdded(e)));
            _disposableCallbacks.Add(QuantumEvent.SubscribeManual((EventBattleActorLinkRemoved e) =>
                WhenBattleActorLinkRemoved(e)));
            _disposableCallbacks.Add(QuantumEvent.SubscribeManual((EventUpdateCutsceneControlledEntities e) =>
                UpdateCutsceneControlledEntitiesEvent(e)));

            _disposableCallbacks.Add(
                QuantumCallback.SubscribeManual((CallbackUpdateView callback) => UpdateView(callback)));
        }
        
        public virtual void Teardown()
        {
            for (int i = 0; i < _disposableCallbacks.Count; i++)
            {
                _disposableCallbacks[i].Dispose();
            }
            _disposableCallbacks.Clear();
            _disposableCallbacks = null;
            globalBindingSource = null;
            matchHandler = null;
        }

        [Serializable]
        public class SyncedCutsceneGrouping
        {
            public int lastUpdatedFrameNumber = -1;
            public SyncedCutsceneSource previousSource;
            public SyncedCutsceneSource currentSource;
            public float accumulatedTimeSinceLastUpdate;
            public bool valid;
            public bool updateControlledEntities;
            public HashSet<QuantumEntityView> viewsUsed = new(2);
        }

        public Dictionary<EntityRef, SyncedCutsceneGrouping> currentSyncedCutscenes = new();
        [NonSerialized] protected List<EntityRef> syncedCutscenesToRemove = new();


        public ObjectPool<SyncedCutsceneGrouping> syncedCutsceneGroupingPool = new ObjectPool<SyncedCutsceneGrouping>(
            () => new SyncedCutsceneGrouping(),
            (scg) => { scg.updateControlledEntities = true; },
            (scg) =>
            {
                scg.lastUpdatedFrameNumber = -1;
                scg.previousSource = default;
                scg.currentSource = default;
                scg.accumulatedTimeSinceLastUpdate = 0;
                scg.valid = false;
                scg.updateControlledEntities = false;
                scg.viewsUsed.Clear();
            },
            null,
            false,
            4);

        protected virtual void UpdateView(CallbackUpdateView callback)
        {
            //Profiler.BeginSample("GlobalCutsceneManager: UpdateView");
            var frame = callback.Game.Frames.Predicted;
            var filter = frame.Filter<SyncedCutsceneSource>();

            foreach (var v in currentSyncedCutscenes) v.Value.valid = false;

            Profiler.BeginSample("Global Cutscene Manager");
            UpdateKnownSyncedCutscenes(callback, filter, frame);
            TickSyncedCutscenes(callback, frame);
            CleanupInvalidCutscenes(frame);
            Profiler.EndSample();
        }

        /// Updates the view's known running cutscenes along with their current values.
        protected virtual void UpdateKnownSyncedCutscenes(CallbackUpdateView callback, ComponentFilter<SyncedCutsceneSource> filter, Frame frame)
        {
            Profiler.BeginSample("Updating Known Synced Cutscenes");
            while (filter.NextUnsafe(out var entityRef, out var syncedCutsceneSource))
            {
                // If we're already tracking the cutscene...
                if (currentSyncedCutscenes.TryGetValue(entityRef, out var dictGrouping))
                {
                    // Cutscene changed, remove it so it gets readded later.
                    if (currentSyncedCutscenes[entityRef].currentSource.cutsceneTag != syncedCutsceneSource->cutsceneTag)
                    {
                        syncedCutscenesToRemove.Add(entityRef);
                        CleanupInvalidCutscenes(frame);
                    }
                    else
                    {
                        // Only values changed, just update them.
                        var hasLdt =
                            frame.Unsafe.TryGetPointer<LocalDeltaTime>(syncedCutsceneSource->sourcePlayer, out var ldt);

                        dictGrouping.valid = true;
                        if (dictGrouping.lastUpdatedFrameNumber != frame.Number
                            && (!hasLdt || (ldt->updatesThisTick > 0)))
                        {
                            dictGrouping.lastUpdatedFrameNumber = frame.Number;
                            dictGrouping.accumulatedTimeSinceLastUpdate = 0;
                            dictGrouping.previousSource = dictGrouping.currentSource;
                            dictGrouping.currentSource = *syncedCutsceneSource;
                        }
                        continue;
                    }
                }

                // Only bother with the cutscene if it's valid for one of our local players.
                if (frame.TryResolveList(syncedCutsceneSource->onlyFor, out var onlyPlayerList)
                    && onlyPlayerList.Count > 0)
                {
                    bool foundLocal = false;
                    foreach (var v in onlyPlayerList)
                    {
                        if(callback.Game.PlayerIsLocal(v) == false) continue;
                        foundLocal = true;
                    }
                    if(foundLocal == false) continue;
                }
                
                // Cutscene wasn't tracked, add it.
                var scg = syncedCutsceneGroupingPool.Get();
                scg.valid = true;
                scg.currentSource = *syncedCutsceneSource;

                currentSyncedCutscenes.Add(entityRef, scg);
            }
            Profiler.EndSample();
        }
        
        protected virtual void TickSyncedCutscenes(CallbackUpdateView callback, Frame frame)
        {
            Profiler.BeginSample("Processing Synced Cutscenes");
            // Check all the cutscenes we know of view-side.
            foreach (var (syncedCutsceneEntity, syncedCutsceneGroup) in currentSyncedCutscenes)
            {
                // Cutscene wasn't found in the sim this frame, mark it for deletion.
                if (!syncedCutsceneGroup.valid)
                {
                    syncedCutscenesToRemove.Add(syncedCutsceneEntity);
                    continue;
                }

                // Get or Create Cutscene Group GameObject for the playing entity.
                if (!entityToCutscenePlayers.TryGetValue(syncedCutsceneGroup.currentSource.sourcePlayer,
                        out var playerCutsceneGrouping) 
                    || playerCutsceneGrouping == null 
                    || playerCutsceneGrouping.sourceKey != syncedCutsceneGroup.currentSource.cutsceneSource)
                {
                    if (playerCutsceneGrouping == null || playerCutsceneGrouping.sourceKey !=
                        syncedCutsceneGroup.currentSource.cutsceneSource)
                    {
                        entityToCutscenePlayers.Remove(syncedCutsceneGroup.currentSource.sourcePlayer);
                    }
                    playerCutsceneGrouping = GetCutsceneGroupFromPool(syncedCutsceneGroup.currentSource.cutsceneSource);
                    if(playerCutsceneGrouping != null) entityToCutscenePlayers.Add(syncedCutsceneGroup.currentSource.sourcePlayer, playerCutsceneGrouping);
                }

                if (playerCutsceneGrouping == null)
                {
                    continue;
                }
                
                if (!playerCutsceneGrouping.CutscenePlayersMap.TryGetValue(syncedCutsceneGroup.currentSource.cutsceneTag,
                        out var gcp))
                {
                    Debug.LogError($"Could not get requested cutscene player this frame. " +
                                   $"source={syncedCutsceneGroup.currentSource.cutsceneSource.ToString()}," +
                                   $"tag={syncedCutsceneGroup.currentSource.cutsceneTag.ToString()}," +
                                   $"player={syncedCutsceneGroup.currentSource.sourcePlayer.ToString()}");
                    continue;
                }

                var playingEntityRef = syncedCutsceneGroup.currentSource.sourcePlayer; 
                //var actorEntityView = viewUpdater.GetView(playingEntityRef);

                LocalDeltaTime* ldt = null;
                var hasLdt = frame.Exists(playingEntityRef)
                             && frame.Unsafe.TryGetPointer<LocalDeltaTime>(syncedCutsceneGroup.currentSource.sourcePlayer, 
                                 out ldt);
                var fdt = frame.DeltaTime.AsFloat;
                int lastFrame = syncedCutsceneGroup.currentSource.frame;
                int currentFrame = syncedCutsceneGroup.currentSource.frame;
                if (syncedCutsceneGroup.previousSource.cutsceneTag == syncedCutsceneGroup.currentSource.cutsceneTag)
                {
                    lastFrame = syncedCutsceneGroup.previousSource.frame;
                }
                float lastFrameTime = lastFrame * fdt;
                float currentFrameTime = currentFrame * fdt;
                
                // Hasn't played yet, initialize cutscene.
                if (gcp.director.state != PlayState.Playing)
                {
                    SetupStandardBindings(callback.Game, playerCutsceneGrouping.bindingSource, playingEntityRef, gcp,
                        syncedCutsceneGroup);
                    SetupPlayerGroupBindingSource(playerCutsceneGrouping.bindingSource, playingEntityRef);
                }
                
                // Controlling Tag Mapped Entities.
                if (frame.Exists(playingEntityRef) 
                    && frame.Unsafe.TryGetPointer<TaggedEntityMapping>(playingEntityRef, out var playerTaggedEntityMapping))
                {
                    var tagEntityMap = frame.ResolveDictionary(playerTaggedEntityMapping->tagToEntityMap);
                    var dd = frame.ResolveDictionary(syncedCutsceneGroup.currentSource.cutsceneControls);

                    foreach (var taggedEntity in tagEntityMap)
                    {
                        var view = viewUpdater.GetView(taggedEntity.Value);
                        if(view == null) continue;
                        //if(!syncedCutsceneGroup.viewsUsed.Contains(view)) SetBindingsForTaggedView(callback.Game, playerCutsceneGrouping, taggedEntity.Key, view);
                        SetBindingsForTaggedView(callback.Game, playerCutsceneGrouping, taggedEntity.Key, view);
                        syncedCutsceneGroup.viewsUsed.Add(view);
                        if (dd.ContainsKey(taggedEntity.Key))
                        {
                            UpdateViewControl(view.gameObject, dd[taggedEntity.Key].controlAnimation, dd[taggedEntity.Key].controlPosition);
                        }
                    }
                }
                
                // Ticking.
                gcp.game = callback.Game;
                gcp.sourceEntityRef = playingEntityRef;
                gcp.autoUpdatePlayRate = false;
                if (gcp.director.state != PlayState.Playing)
                    gcp.PlayCutscene(callback.Game, playerCutsceneGrouping.bindingSource, DirectorUpdateMode.Manual);
                
                var effectiveFixedDT = hasLdt
                    ? callback.Game.Frames.Predicted.DeltaTime.AsFloat / ldt->multiplier.AsFloat
                    : callback.Game.Frames.Predicted.DeltaTime.AsFloat;
                var alpha = syncedCutsceneGroup.accumulatedTimeSinceLastUpdate / (effectiveFixedDT);

                gcp.CutsceneTime = Mathf.Lerp(lastFrameTime, currentFrameTime, alpha);
                gcp.director.playableGraph.Evaluate(effectiveFixedDT);
                gcp.PostTimelineTicked();

                syncedCutsceneGroup.accumulatedTimeSinceLastUpdate += Time.deltaTime;
            }
            Profiler.EndSample();
        }

        protected virtual void SetBindingsForTaggedView(QuantumGame callbackGame, CutsceneGrouping playerCutsceneGrouping, AssetRef<Tag> taggedEntityKey, QuantumEntityView view)
        {
        }

        protected virtual void CleanupInvalidCutscenes(Frame frame)
        {
            Profiler.BeginSample("Cleanup");
            foreach (var invalidSyncedGroup in syncedCutscenesToRemove)
            {
                var syncedCutsceneGroup = currentSyncedCutscenes[invalidSyncedGroup];

                var dd = frame.ResolveDictionary(syncedCutsceneGroup.currentSource.cutsceneControls);

                foreach (var v in syncedCutsceneGroup.viewsUsed)
                    ReturnExclusiveControlOfFighterAnimation(v.gameObject);
                
                if (entityToCutscenePlayers.TryGetValue(syncedCutsceneGroup.currentSource.sourcePlayer, out var playerCutsceneGrouping))
                {
                    ReturnCutsceneGroupToPool(playerCutsceneGrouping);
                }
                
                entityToCutscenePlayers.Remove(syncedCutsceneGroup.currentSource.sourcePlayer);
                
                syncedCutsceneGroupingPool.Release(syncedCutsceneGroup);
                currentSyncedCutscenes.Remove(invalidSyncedGroup);
            }
            syncedCutscenesToRemove.Clear();
            Profiler.EndSample();
        }

        protected virtual void UpdateViewControl(GameObject view, bool controlAnimation, bool controlPosition)
        {
            if (controlPosition) TakeControlOfEntityModelPosition(view);
            else ReturnControlOfEntityModelPosition(view);

            if (controlAnimation)
            {
                TakeExclusiveControlOfFighterAnimation(view);
            }
            else
            {
                ReturnExclusiveControlOfFighterAnimation(view);
            }
        }

        protected virtual void UpdateCutsceneControlledEntitiesEvent(EventUpdateCutsceneControlledEntities callback)
        {
            foreach (var (syncedCutsceneEntity, syncedCutsceneGroup) in currentSyncedCutscenes)
            {
                if (syncedCutsceneGroup.currentSource.sourcePlayer != callback.cutscenePlayingEntity
                    || syncedCutsceneGroup.currentSource.cutsceneTag != callback.cutsceneTag) continue;

                syncedCutsceneGroup.updateControlledEntities = true;
                break;
            }
        }

        protected virtual void SetupStandardBindings(QuantumGame game, CutsceneBindingSource bindingSource,
            EntityRef cutsceneEntityRef, ActorCutscenePlayer gameCutscenePlayer, SyncedCutsceneGrouping scg)
        {
            
        }

        protected virtual void WhenBattleActorLinkAdded(EventBattleActorLinkAdded callback)
        {
            if (viewUpdater == null)
                viewUpdater = GameObject.FindAnyObjectByType<QuantumEntityViewUpdater>();

            if (!QuantumUnityDB.TryGetGlobalAsset(callback.battleActorDefinitionReference, out var charaDefinitionAsset))
                return;
            RegisterCutsceneGroupPrefab(charaDefinitionAsset, charaDefinitionAsset.cutsceneGroupingPrefab?.GetComponent<CutsceneGrouping>());
            
            EventKey key = (EventKey)callback;
            _unconfirmedLinkCutsceneGrouping.Add(key, callback);
        }
        
        protected virtual void WhenBattleActorLinkRemoved(EventBattleActorLinkRemoved callback)
        {
            EventKey key = (EventKey)callback;
            _unconfirmedUnlinkCutsceneGrouping.Add(key, callback);
        }

        protected virtual CutsceneGrouping GetCutsceneGroupFromPool(AssetRef source)
        {
            if (singletonCutsceneGroupings.TryGetValue(source, out var singletonCutsceneGrouping))
            {
                singletonCutsceneGrouping.bindingSource.parent = globalBindingSource;
                return singletonCutsceneGrouping;
            }
            
            if (!cutsceneGroupingPools.ContainsKey(source))
            {
                Debug.LogError($"Cutscene Pools do not contain source {source}.");
                return null;
            }
            CutsceneGrouping cg = null;
            if(!cutsceneGroupingPools.ContainsKey(source) || cutsceneGroupingPools[source].Count == 0)
            {
                if (!cutsceneGroupingPrefabs.TryGetValue(source, out var cgPrefab))
                {
                    Debug.LogError($"No cutscene group prefab found for source={source}.");
                    return null;
                }
                cg = GameObject.Instantiate(cgPrefab, Vector3.zero, Quaternion.identity);
                cg.sourceKey = source;
                cg.bindingSource = new CutsceneBindingSource();
            }
            else
            {
                cg = cutsceneGroupingPools[source][0];
                cutsceneGroupingPools[source].Remove(cg);
            }

            cg.bindingSource.parent = globalBindingSource;
            return cg;
        }

        protected virtual void ReturnCutsceneGroupToPool(CutsceneGrouping cg)
        {
            cg.StopAll();
            if (singletonCutsceneGroupings.ContainsKey(cg.sourceKey))
            {
                Debug.Log("Stopped singleton cutscene group.", cg.gameObject);
                return;
            }
            if (cg.bindingSource == null || !cutsceneGroupingPools.ContainsKey(cg.sourceKey))
            {
                GameObject.Destroy(cg);
                return;
            }
            cutsceneGroupingPools[cg.sourceKey].Add(cg);
        }

        public virtual void RegisterCutsceneGroupPrefab(AssetRef source, CutsceneGrouping groupPrefab)
        {
            if (groupPrefab == null) return;
            cutsceneGroupingPrefabs.TryAdd(source, groupPrefab);
            if(!cutsceneGroupingPools.ContainsKey(source)) cutsceneGroupingPools.Add(source, new List<CutsceneGrouping>());
        }

        protected virtual void SetupPlayerGroupBindingSource(CutsceneBindingSource bindingSource, EntityRef sourceEntityRef,
            bool ignoreIfAlreadyHaveMapping = true)
        {
            
        }
        
        protected virtual void WhenEventConfirmed(CallbackEventConfirmed callback)
        {
            if (_unconfirmedLinkCutsceneGrouping.ContainsKey(callback.EventKey))
            {
                _unconfirmedLinkCutsceneGrouping.Remove(callback.EventKey);
            }
            else if (_unconfirmedUnlinkCutsceneGrouping.ContainsKey(callback.EventKey))
            {
                ConfirmUnlinkCutscenePlayerGroup(_unconfirmedUnlinkCutsceneGrouping[callback.EventKey].entity,
                    _unconfirmedUnlinkCutsceneGrouping[callback.EventKey].battleActorDefinitionReference);
                _unconfirmedUnlinkCutsceneGrouping.Remove(callback.EventKey);
            }
        }

        protected virtual void WhenEventCanceled(CallbackEventCanceled callback)
        {
            if (_unconfirmedLinkCutsceneGrouping.ContainsKey(callback.EventKey))
            {
                ConfirmUnlinkCutscenePlayerGroup(_unconfirmedLinkCutsceneGrouping[callback.EventKey].entity,
                    _unconfirmedLinkCutsceneGrouping[callback.EventKey].battleActorDefinitionReference);
                _unconfirmedLinkCutsceneGrouping.Remove(callback.EventKey);
            }
            else if (_unconfirmedUnlinkCutsceneGrouping.ContainsKey(callback.EventKey))
            {
                _unconfirmedUnlinkCutsceneGrouping.Remove(callback.EventKey);
            }
        }
        
        protected virtual void ConfirmUnlinkCutscenePlayerGroup(EntityRef entity,
            AssetRef<BattleActorDefinition> charaDefinitionReference)
        {
            /*
            if (!entityToCutscenePlayers.ContainsKey(entity)) return;

            entityToCutscenePlayers[entity].StopAll();

            cutsceneGroupingPools.TryAdd(charaDefinitionReference.Id, new List<CutsceneGrouping>());
            cutsceneGroupingPools[charaDefinitionReference.Id].Add(entityToCutscenePlayers[entity]);

            entityToCutscenePlayers.Remove(entity);*/
        }

        public virtual void TakeExclusiveControlOfFighterAnimation(GameObject entityView)
        {
            
        }

        public virtual void ReturnExclusiveControlOfFighterAnimation(GameObject entityView)
        {
            
        }

        public virtual void TakeControlOfEntityModelPosition(GameObject entityView)
        {
            
        }

        public virtual void ReturnControlOfEntityModelPosition(GameObject entityView)
        {
            
        }

        public virtual void ReturnControlOfEntityModelAnimation(GameObject entityView)
        {
            
        }

        protected virtual void ReleaseEntityFromCutsceneEvent(EventReleaseEntityFromCutsceneAnimationControl callback)
        {
            EventKey key = (EventKey)callback;

            foreach (var (syncedCutsceneEntity, syncedCutsceneGroup) in currentSyncedCutscenes)
            {
                if (syncedCutsceneEntity != callback.cutscenePlayer) continue;

                var ev = GetBindingObject(callback.Game, syncedCutsceneEntity, callback.entityTag);
                if (ev == null) return;

                ReturnExclusiveControlOfFighterAnimation(ev);
                break;
            }
        }

        public virtual GameObject GetBindingObject(QuantumGame game, EntityRef cutscenePlayer, AssetRef<Tag> objectTag)
        {
            return null;
        }
    }
}