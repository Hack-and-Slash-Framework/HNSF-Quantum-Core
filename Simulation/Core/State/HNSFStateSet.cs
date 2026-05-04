using System;
using System.Linq;
using System.Collections.Generic;
using Photon.Deterministic;
using Quantum;
#if QUANTUM_UNITY
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
#endif

namespace HnSF.core.state
{
    public partial class HNSFStateSet : AssetObject
    {
        public string Label;
        public List<AssetRef<HNSFState>> states = new List<AssetRef<HNSFState>>();
        
        // Moveset : State Tag : State
        [NonSerialized] public Dictionary<AssetRef<Tag>, Dictionary<AssetRef<Tag>, AssetRef<HNSFState>>> statesByTag =
            new Dictionary<AssetRef<Tag>, Dictionary<AssetRef<Tag>, AssetRef<HNSFState>>>();
        
        public AssetRef<HNSFSpecialSet>[] specials;

        public AssetRef<HNSFSpecialSet>[] defaultSpecials;

        public AssetRef<HNSFStateSet> template;

        public AssetRef<Tag>[] movesets = new AssetRef<Tag>[1];

        public bool debug;

        public AssetRef<BattleActorDefinition> previewActor;

        public bool HasStateWithTag(AssetRef<Tag> movesetTag, AssetRef<Tag> stateTag)
        {
            return statesByTag.TryGetValue(movesetTag, out var movesetToStatesByTag) && movesetToStatesByTag.ContainsKey(stateTag);
        }
        
        public AssetRef<HNSFState> AttemptGetStateByTag(AssetRef<Tag> movesetTag, AssetRef<Tag> stateTag)
        {
            if (statesByTag.ContainsKey(movesetTag) == false) return default;
            return statesByTag[movesetTag].GetValueOrDefault(stateTag, default);
        }
        
        public bool AttemptGetStateByTag(AssetRef<Tag> movesetTag, AssetRef<Tag> stateTag, out AssetRef<HNSFState> state)
        {
            state = default;
            if (statesByTag.ContainsKey(movesetTag) == false) return false;
            state = statesByTag[movesetTag].GetValueOrDefault(stateTag, default);
            return state != default;
        }
        
        public override void Loaded(IResourceManager resourceManager, Native.Allocator allocator)
        {
            statesByTag.Clear();
            
            for (int i = 0; i < states.Count; i++)
            {
                if(debug) Log.Debug($"Registration: {Label} : {states[i]}");
                resourceManager.LoadAssetAsync(states[i].Id);
                if (!resourceManager.TryGetAsset(states[i].Id, out HNSFState state)) continue;
                if (state.sharedStateTag == default || !state.sharedStateTag.IsValid) continue;
                
                var movesetTagsToRegister = new List<AssetRef<Tag>>();

                if (state.applyToAllMovesets)
                {
                    foreach(var m in movesets) movesetTagsToRegister.Add(m);
                }
                else
                {
                    foreach(var m in state.movesetTags) movesetTagsToRegister.Add(m);
                }

                if(debug) Log.Debug($"Registration has Moveset Count of {movesetTagsToRegister.Count}");
                foreach (var movesetTag in movesetTagsToRegister)
                {
                    statesByTag.TryAdd(movesetTag, new Dictionary<AssetRef<Tag>, AssetRef<HNSFState>>());
                    statesByTag[movesetTag][state.sharedStateTag] = state;
                }
            }
            
            if(!template.IsValid || !resourceManager.LoadAssetAsync(template.Id) || !resourceManager.TryGetAsset(template.Id, out HNSFStateSet templateStateSet)) return;
            
            if(debug) Log.Debug($"Got template {templateStateSet.Label}");

            RecursivelyAddTemplateTaggedStates(resourceManager, templateStateSet);
        }

        protected void RecursivelyAddTemplateTaggedStates(IResourceManager resourceManager, HNSFStateSet templateStateSet)
        {
            if(debug) Log.Debug($"{templateStateSet.Label} has state count of {templateStateSet.states.Count}.");
            for (int i = 0; i < templateStateSet.states.Count; i++)
            {
                resourceManager.LoadAssetAsync(templateStateSet.states[i].Id);
                if (!resourceManager.TryGetAsset(templateStateSet.states[i].Id, out HNSFState state))
                {
                    if(debug) Log.Debug("Could not get state asset.");
                    continue;
                }
                if (state.sharedStateTag == default || !state.sharedStateTag.IsValid)
                {
                    if(debug) Log.Debug($"State {state.Label} has no shared state tag. Skipping. In stateset {Label}");
                    continue;
                }

                if(debug) Log.Debug($"Looping through moveset count of {movesets.Length}");
                foreach (var movesetTag in movesets)
                {
                    if(debug) Log.Debug($"Moveset tag {movesetTag}");
                    if (state.applyToAllMovesets == false && state.movesetTags.Contains(movesetTag) == false) continue;
                    statesByTag.TryAdd(movesetTag, new Dictionary<AssetRef<Tag>, AssetRef<HNSFState>>());
                    var movesetTaggedStateDict = statesByTag[movesetTag];
                    movesetTaggedStateDict.TryAdd(state.sharedStateTag, state);
                    if (debug) Log.Debug($"Added State {state.Label} to Moveset {movesetTag}");
                }
            }

            if (!templateStateSet.template.IsValid || !resourceManager.LoadAssetAsync(templateStateSet.template.Id) ||
                !resourceManager.TryGetAsset(templateStateSet.template.Id, out HNSFStateSet parentTemplateStateSet))
                return;
            RecursivelyAddTemplateTaggedStates(resourceManager, parentTemplateStateSet);
            if(debug) Log.Debug($"{templateStateSet.Label} got parent template {templateStateSet.Label}");
        }
#if QUANTUM_UNITY
        [System.Serializable]
        public class StateGrouping
        {
            public string label;
            public List<AssetRef<HNSFState>> states = new List<AssetRef<HNSFState>>();
        }
    
#if UNITY_EDITOR
        public List<StateGrouping> stateGroups = new List<StateGrouping>();
#endif
        
        public void OnValidate()
        {
            if (!Application.isEditor || Application.isPlaying) return;
            RefreshStateList();
        }

        public void RefreshStateList()
        {
#if UNITY_EDITOR
            var stateList = new List<AssetRef<HNSFState>>();
        
            foreach (var stateGrouping in stateGroups)
            {
                foreach (var state in stateGrouping.states)
                {
                    stateList.Add(state);
                }
            }

            states.Clear();
            states.AddRange(stateList);
            
            EditorUtility.SetDirty(this);
#endif
        }
#endif
    }
}