using Photon.Deterministic;
using System;
using System.Collections.Generic;
using System.Linq;
using HnSF.core.state.actions;
using HnSF.core.state.decisions;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF.core.state
{
    [System.Serializable]
    public unsafe partial class HNSFState : AssetObject
    {
        [Header("General")]
        public string Label;
        public string displayName;
        public AssetRef<Tag>[] tags = Array.Empty<AssetRef<Tag>>();
        public bool useBaseState;
        public bool processBaseStateFirst;
        public AssetRef<HNSFState> baseState;
        public int totalFrames = 10;
        public bool autoIncrement = true;
        public bool autoLoop = true;
        public int autoLoopFrame = 1;
        public bool clearInputBuffer;
        public bool dontClearHitEntities;
        public AssetRef<Tag> stateType;
        public int moveHitCounter = 1;
        public bool applyToAllMovesets = true;
        [DrawIf(nameof(applyToAllMovesets), false)]
        public AssetRef<Tag>[] movesetTags = new AssetRef<Tag>[1];
        public AssetRef<Tag> sharedStateTag;
        public StateGroundedType initialGroundedState;
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HNSFStateAction[] data = Array.Empty<HNSFStateAction>();

        [Header("Combo Decay")] 
        public AssetRef<HNSFState> countsAsStateForDecay;
        public bool incrementStateCounter = true;
        
        [Header("Conditions")]
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HNSFStateDecision[] conditions = Array.Empty<HNSFStateDecision>();

        public AssetRef<InputConditionListAsset>[] defaultInputConditions = Array.Empty<AssetRef<InputConditionListAsset>>();

        public List<HNSFStateIgnoredAction> ignoredActions = new List<HNSFStateIgnoredAction>();

        // Realtime Data
        [NonSerialized] public Dictionary<AssetRef<HNSFState>, HashSet<int>> ignoredActionsDictionary = new Dictionary<AssetRef<HNSFState>, HashSet<int>>();
        [NonSerialized] public HashSet<AssetRef<Tag>> allTags = new HashSet<AssetRef<Tag>>();
        [NonSerialized] public AssetRef<Tag> realSharedStateTag;
        
        public override void Loaded(IResourceManager resourceManager, Native.Allocator allocator)
        {
            base.Loaded(resourceManager, allocator);
            ignoredActionsDictionary.Clear();
            BuildIgnoreDictionary(resourceManager, ignoredActionsDictionary);
            allTags.Clear();
            CollectAllTags(resourceManager, allTags);
            GetRealSharedStateTagRecursive(resourceManager, out realSharedStateTag);
        }
        
        private void GetRealSharedStateTagRecursive(IResourceManager resourceManager, out AssetRef<Tag> gotRealSharedStateTag)
        {
            gotRealSharedStateTag = sharedStateTag;
            
            if (gotRealSharedStateTag != default || useBaseState == false || baseState == default)
                return;
            
            if (resourceManager.TryGetAsset(baseState, out var baseStateAsset))
            {
                baseStateAsset.GetRealSharedStateTagRecursive(resourceManager, out gotRealSharedStateTag);
            }
        }

        protected virtual void CollectAllTags(IResourceManager resourceManager, HashSet<AssetRef<Tag>> tagSet)
        {
            foreach(var t in tags) tagSet.Add(t);
            if(useBaseState && resourceManager.TryGetAsset(baseState, out var baseStateAsset)) baseStateAsset.CollectAllTags(resourceManager, tagSet);
        }
        
        protected virtual void BuildIgnoreDictionary(IResourceManager resourceManager, Dictionary<AssetRef<HNSFState>, HashSet<int>> dict)
        {
            foreach (var ia in ignoredActions)
            {
                if(!dict.ContainsKey(ia.stateRef)) dict.Add(ia.stateRef, new HashSet<int>());
                dict[ia.stateRef].Add(ia.actionId);
            }
            
            if(useBaseState && resourceManager.TryGetAsset(baseState, out var baseStateAsset)) baseStateAsset.BuildIgnoreDictionary(resourceManager, dict);
        }

        /// <summary>
        /// Executes the state with the information given.
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="deltaTime"></param>
        /// <param name="hfsmData"></param>
        /// <param name="entity"></param>
        /// <param name="whitelist"></param>
        /// <param name="blacklist"></param>
        /// <returns>If a transition occurs, returns True.</returns>
        public Boolean Execute(ref FrameThreadSafe frame, FP deltaTime, HNSFStateAgentData* hnsfData, EntityRef entity,
            ref HNSFStateContext context)
        {
            return Execute(ref frame, deltaTime, hnsfData, entity, ref context, ignoredActionsDictionary);
        }
        
        public Boolean Execute(ref FrameThreadSafe frame, FP deltaTime, HNSFStateAgentData* hnsfData, EntityRef entity, ref HNSFStateContext context, Dictionary<AssetRef<HNSFState>, HashSet<int>> ignored)
        {
            if (useBaseState && processBaseStateFirst)
            {
                var bs = frame.FindAsset<HNSFState>(this.baseState.Id);
                bs.Execute(ref frame, deltaTime, hnsfData, entity, ref context, ignored);
            }
            
            for (int i = 0; i < data.Length; i++)
            {
                if(ignored != null && ignored.ContainsKey(this) && ignored[this].Contains(data[i].id)) continue;
                data[i].Execute(ref frame, entity, hnsfData, ref context);
            }
            
            if (useBaseState && !processBaseStateFirst)
            {
                var bs = frame.FindAsset<HNSFState>(this.baseState.Id);
                bs.Execute(ref frame, deltaTime, hnsfData, entity, ref context, ignored);
            }
            return false;
        }

        public bool CheckConditions(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (conditions == null || conditions.Length == 0) return true;
            foreach (var d in conditions)
            {
                if (d.Decide(frame, entity, ref stateContext) == false) return false;
            }
            return true;
        }
        
        public HNSFStateAction GetStateActionById(int id)
        {
            foreach (var t in data)
            {
                if (t.id == id) return t;
            }

            return default;
        }

        public int GetStateActionIndexById(int id)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].id == id) return i;
            }
            return -1;
        }

        public virtual int GenerateValidActionId()
        {
            int validId = 0;

            for (int i = 0; i < data.Length; i++)
            {
                validId = Math.Max(validId, data[i].GetHighestId());
            }
            
            return validId+1;
        }

        public virtual void CopyTo(HNSFState target)
        {
            target.Label = Label;
            target.displayName = displayName;
            target.tags = new AssetRef<Tag>[tags.Length];
            Array.Copy(tags, target.tags, tags.Length);
            target.useBaseState = useBaseState;
            target.processBaseStateFirst = processBaseStateFirst;
            target.baseState = baseState;
            target.totalFrames = totalFrames;
            target.autoIncrement = autoIncrement;
            target.autoLoop = autoLoop;
            target.autoLoopFrame = autoLoopFrame;
            target.clearInputBuffer = clearInputBuffer;
            target.dontClearHitEntities = dontClearHitEntities;
            target.stateType = stateType;
            target.moveHitCounter = moveHitCounter;
            target.applyToAllMovesets = applyToAllMovesets;
            target.movesetTags = new AssetRef<Tag>[movesetTags.Length];
            Array.Copy(movesetTags, target.movesetTags, movesetTags.Length);
            target.sharedStateTag = sharedStateTag;

            target.defaultInputConditions = new AssetRef<InputConditionListAsset>[defaultInputConditions.Length];
            Array.Copy(defaultInputConditions, target.defaultInputConditions, defaultInputConditions.Length);
            target.ignoredActions = ignoredActions.ToList();
        }

        public virtual void CopyDataTo(HNSFState target)
        {
            target.data = new HNSFStateAction[data.Length];
            target.conditions = new HNSFStateDecision[conditions.Length];
            
            for (int i = 0; i < data.Length; i++)
            {
                target.data[i] = data[i].Copy();
            }

            for (int i = 0; i < conditions.Length; i++)
            {
                target.conditions[i] = conditions[i].Copy();
            }
        }
    }
}