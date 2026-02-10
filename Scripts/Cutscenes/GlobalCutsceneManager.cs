using System;
using System.Collections.Generic;
using Quantum;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Pool;

namespace HnSF
{
    [System.Serializable]
    public unsafe class GlobalCutsceneManager
    {
        protected List<IDisposable> _disposableCallbacks = new List<IDisposable>();

        protected Dictionary<EventKey, EventBattleActorLinkAdded> _unconfirmedLinkCutsceneGrouping = new();
        protected Dictionary<EventKey, EventBattleActorLinkRemoved> _unconfirmedUnlinkCutsceneGrouping = new();

        public QuantumEntityViewUpdater viewUpdater;

        public Dictionary<EntityRef, CutsceneGrouping> entityToCutscenePlayers = new();

        public Dictionary<AssetRef<BattleActorDefinition>, List<CutsceneGrouping>> cutsceneGroupingPool = new();

        public CutsceneBindingSource globalBindingSource;

        public MatchHandlerBase matchHandler;

        public virtual void Initialize(MatchHandlerBase mHandler, CutsceneBindingSource bindingSource = null)
        {
            matchHandler = mHandler;
            globalBindingSource = bindingSource ?? new CutsceneBindingSource();
            _disposableCallbacks.Add(
                QuantumCallback.SubscribeManual((CallbackEventCanceled c) => WhenEventCanceled(c)));
            _disposableCallbacks.Add(
                QuantumCallback.SubscribeManual((CallbackEventConfirmed c) => WhenEventConfirmed(c)));
            _disposableCallbacks.Add(QuantumEvent.SubscribeManual((EventBattleActorLinkAdded e) =>
                LinkCutscenePlayerGroup(e)));
            _disposableCallbacks.Add(QuantumEvent.SubscribeManual((EventBattleActorLinkRemoved e) =>
                UnlinkCutscenePlayerGroup(e)));
            _disposableCallbacks.Add(QuantumEvent.SubscribeManual((EventUpdateCutsceneControlledEntities e) =>
                UpdateCutsceneControlledEntitiesEvent(e)));

            _disposableCallbacks.Add(
                QuantumCallback.SubscribeManual((CallbackUpdateView callback) => UpdateView(callback)));
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

            //Profiler.BeginSample("Finding Synced Cutscenes");
            while (filter.NextUnsafe(out var entityRef, out var syncedCutsceneSource))
            {
                if (currentSyncedCutscenes.TryGetValue(entityRef, out var dictGrouping))
                {
                    // Cutscene changed
                    if (currentSyncedCutscenes[entityRef].currentSource.cutsceneTag !=
                        syncedCutsceneSource->cutsceneTag)
                    {
                        var syncedCutsceneGroup = currentSyncedCutscenes[entityRef];

                        foreach (var v in syncedCutsceneGroup.viewsUsed)
                            ReturnExclusiveControlOfFighterAnimation(v.gameObject);

                        if (entityToCutscenePlayers.TryGetValue(syncedCutsceneGroup.currentSource.sourcePlayer,
                                out var playerCutsceneGrouping)
                            && playerCutsceneGrouping.cutscenePlayers.TryGetValue(
                                syncedCutsceneGroup.currentSource.cutsceneTag,
                                out var gcp))
                        {
                            gcp.StopCutscene(pause: false);
                        }

                        syncedCutsceneGroupingPool.Release(syncedCutsceneGroup);
                        currentSyncedCutscenes.Remove(entityRef);
                    }
                    else
                    {
                        // Update values.
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
            //Profiler.EndSample();

            //Profiler.BeginSample("Processing Synced Cutscenes");
            foreach (var (syncedCutsceneEntity, syncedCutsceneGroup) in currentSyncedCutscenes)
            {
                if (!syncedCutsceneGroup.valid)
                {
                    syncedCutscenesToRemove.Add(syncedCutsceneEntity);
                    continue;
                }

                if (!entityToCutscenePlayers.TryGetValue(syncedCutsceneGroup.currentSource.sourcePlayer,
                        out var playerCutsceneGrouping)) continue;
                if (!playerCutsceneGrouping.cutscenePlayers.TryGetValue(syncedCutsceneGroup.currentSource.cutsceneTag,
                        out var gcp)) continue;

                var cutscenePlayer = viewUpdater.GetView(syncedCutsceneGroup.currentSource.sourcePlayer);
                if (!cutscenePlayer) continue;

                var hasLdt =
                    frame.Unsafe.TryGetPointer<LocalDeltaTime>(syncedCutsceneGroup.currentSource.sourcePlayer,
                        out var ldt);
                var fdt = callback.Game.Frames.Predicted.DeltaTime.AsFloat;

                int lastFrame = syncedCutsceneGroup.currentSource.frame;
                int currentFrame = syncedCutsceneGroup.currentSource.frame;

                if (syncedCutsceneGroup.previousSource.cutsceneTag == syncedCutsceneGroup.currentSource.cutsceneTag)
                {
                    lastFrame = syncedCutsceneGroup.previousSource.frame;
                }

                float lastFrameTime = lastFrame * fdt;
                float currentFrameTime = currentFrame * fdt;
                
                if (gcp.director.state != PlayState.Playing)
                {
                    syncedCutsceneGroup.viewsUsed.Add(cutscenePlayer);
                    SetupStandardBindings(callback.Game, playerCutsceneGrouping.bindingSource, cutscenePlayer, gcp,
                        syncedCutsceneGroup);
                    if (gcp.takeExclusiveControl) TakeExclusiveControlOfFighterAnimation(cutscenePlayer.gameObject);
                    SetupPlayerGroupBindingSource(playerCutsceneGrouping.bindingSource, cutscenePlayer);
                }

                if (syncedCutsceneGroup.updateControlledEntities)
                {
                    var dd = frame.ResolveDictionary(syncedCutsceneGroup.currentSource.cutsceneControls);

                    foreach (var cce in dd)
                    {
                        var view = GetBindingObject(callback.Game, syncedCutsceneGroup.currentSource.sourcePlayer,
                            cce.Key);
                        if (!view) continue;
                        syncedCutsceneGroup.viewsUsed.Add(view.GetComponent<QuantumEntityView>());
                        UpdateViewControl(view, cce.Value.controlAnimation, cce.Value.controlPosition);
                    }
                }

                syncedCutsceneGroup.updateControlledEntities = false;

                gcp.playingEntityView = cutscenePlayer;
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
            //.EndSample();

            //Profiler.BeginSample("Cleanup");
            foreach (var invalidSyncedGroup in syncedCutscenesToRemove)
            {
                var syncedCutsceneGroup = currentSyncedCutscenes[invalidSyncedGroup];

                var dd = frame.ResolveDictionary(syncedCutsceneGroup.currentSource.cutsceneControls);

                foreach (var v in syncedCutsceneGroup.viewsUsed)
                    ReturnExclusiveControlOfFighterAnimation(v.gameObject);

                if (entityToCutscenePlayers.TryGetValue(syncedCutsceneGroup.currentSource.sourcePlayer,
                        out var playerCutsceneGrouping)
                    && playerCutsceneGrouping.cutscenePlayers.TryGetValue(syncedCutsceneGroup.currentSource.cutsceneTag,
                        out var gcp))
                {
                    gcp.StopCutscene(pause: false);
                }

                syncedCutsceneGroupingPool.Release(syncedCutsceneGroup);
                currentSyncedCutscenes.Remove(invalidSyncedGroup);
            }

            syncedCutscenesToRemove.Clear();
            //Profiler.EndSample();

            //Profiler.EndSample();
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
            QuantumEntityView cutsceneEntity, ActorCutscenePlayer gameCutscenePlayer, SyncedCutsceneGrouping scg)
        {
            
        }

        protected virtual void LinkCutscenePlayerGroup(EventBattleActorLinkAdded callback)
        {
            if (viewUpdater == null)
                viewUpdater = GameObject.FindAnyObjectByType<QuantumEntityViewUpdater>();

            if (!QuantumUnityDB.TryGetGlobalAsset(callback.battleActorDefinitionReference, out var charaDefinitionAsset))
                return;
            if (entityToCutscenePlayers.ContainsKey(callback.entity)) return;
            if (charaDefinitionAsset.cutsceneGroupingPrefab == null) return;

            CutsceneGrouping cg = null;
            if (cutsceneGroupingPool.ContainsKey(callback.battleActorDefinitionReference) &&
                cutsceneGroupingPool[callback.battleActorDefinitionReference].Count > 0)
            {
                cg = cutsceneGroupingPool[callback.battleActorDefinitionReference][0];
                cutsceneGroupingPool[callback.battleActorDefinitionReference].RemoveAt(0);
            }
            else
            {
                cg = GameObject
                    .Instantiate(charaDefinitionAsset.cutsceneGroupingPrefab, Vector3.zero, Quaternion.identity)
                    .GetComponent<CutsceneGrouping>();
            }

            cg.bindingSource = new CutsceneBindingSource()
            {
                parent = globalBindingSource
            };

            SetupPlayerGroupBindingSource(cg.bindingSource, viewUpdater.GetView(callback.entity));

            entityToCutscenePlayers.Add(callback.entity, cg);

            EventKey key = (EventKey)callback;
            _unconfirmedLinkCutsceneGrouping.Add(key, callback);
        }

        protected virtual void UnlinkCutscenePlayerGroup(EventBattleActorLinkRemoved callback)
        {
            EventKey key = (EventKey)callback;
            _unconfirmedUnlinkCutsceneGrouping.Add(key, callback);
        }

        protected virtual void SetupPlayerGroupBindingSource(CutsceneBindingSource bindingSource, QuantumEntityView view,
            bool ignoreIfAlreadyHaveMapping = true)
        {
            
        }

        protected virtual void ConfirmUnlinkCutscenePlayerGroup(EntityRef entity,
            AssetRef<BattleActorDefinition> charaDefinitionReference)
        {
            if (!entityToCutscenePlayers.ContainsKey(entity)) return;

            entityToCutscenePlayers[entity].StopAll();

            cutsceneGroupingPool.TryAdd(charaDefinitionReference, new List<CutsceneGrouping>());
            cutsceneGroupingPool[charaDefinitionReference].Add(entityToCutscenePlayers[entity]);

            entityToCutscenePlayers.Remove(entity);
        }

        protected virtual void Breakdown()
        {
            for (int i = 0; i < _disposableCallbacks.Count; i++)
            {
                _disposableCallbacks[i].Dispose();
            }

            _disposableCallbacks.Clear();
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