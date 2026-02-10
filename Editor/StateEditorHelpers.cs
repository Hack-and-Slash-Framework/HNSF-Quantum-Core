using System;
using System.Collections.Generic;
using System.Linq;
using HnSF.core.state;
using HnSF.core.state.actions;
using Quantum;
using UnityEditor;
using UnityEngine;

namespace HnSF
{
    public static partial class StateEditorHelpers
    {
        public static bool StateWithTagInAllMovesets(AssetRef<Tag> sharedStateTag, HNSFStateSet stateSet)
        {
            foreach (var moveset in stateSet.movesets)
            {
                bool foundForMoveset = false;
                for (int i = 0; i < stateSet.states.Count; i++)
                {
                    if (!QuantumUnityDB.TryGetGlobalAssetEditorInstance(stateSet.states[i], out var state)) continue;
                    if (state.sharedStateTag == sharedStateTag && (state.applyToAllMovesets || state.movesetTags.Contains(moveset)) )
                        foundForMoveset = true;
                }

                if (foundForMoveset == false) return false;
            }
            return true;
        }
        
        public static bool StateWithTagExistsForMoveset(AssetRef<Tag> sharedStateTag, AssetRef<Tag> movesetTag, HNSFStateSet stateSet)
        {
            bool foundForMoveset = false;
            for (int i = 0; i < stateSet.states.Count; i++)
            {
                if (!QuantumUnityDB.TryGetGlobalAssetEditorInstance(stateSet.states[i], out var state)) continue;
                if (state.sharedStateTag == sharedStateTag && (state.applyToAllMovesets || state.movesetTags.Contains(movesetTag)))
                {
                    foundForMoveset = true;
                    break;
                }
            }

            return foundForMoveset;
        }
        
        /*
        public static bool StateWithTagNotCurrentlyInStateSet(Tag sharedStateTag, int i)
        {
            for (int w = 0; w < setToGroupToStateList[i].Count; w++)
            {
                var sa = setToGroupToStateList[i][w];

                for (int a = 0; a < sa.Count; a++)
                {
                    if (sa[a].Item1 == default) continue;
                    if (sa[a].Item1.sharedStateTag == sharedStateTag) return false;
                }
            }
            return true;
        }*/

        public static bool IsIgnored(HNSFState workingState, HNSFState stateAsset, HNSFStateAction stateActionAsset)
        {
            return IsIgnoredByWorkingState(workingState, stateAsset, stateActionAsset) || IsIgnoredInBaseStates(workingState, stateAsset, stateActionAsset) || AreParentsIgnored(workingState, stateAsset, stateActionAsset);
        }
        
        public static bool AreParentsIgnored(HNSFState workingState, HNSFState stateAsset, HNSFStateAction stateActionAsset)
        {
            var parent = stateActionAsset.parent;
            
            while (parent != null)
            {
                if (IsIgnoredSelf(workingState, stateAsset, parent)) return true;
                parent = parent.parent;
            }
            return false;
        }

        public static bool IsIgnoredSelf(HNSFState workingState, HNSFState stateAsset, HNSFStateAction stateActionAsset)
        {
            return IsIgnoredByWorkingState(workingState, stateAsset, stateActionAsset) || IsIgnoredInBaseStates(workingState, stateAsset, stateActionAsset);
        }
        
        public static bool IsIgnoredByWorkingState(HNSFState workingState, HNSFState stateAsset, HNSFStateAction stateActionAsset)
        {
            return workingState.ignoredActions.Exists((x) =>
                x.stateRef == stateAsset && x.actionId == stateActionAsset.id);
        }

        public static bool IsIgnoredInBaseStates(HNSFState workingState, HNSFState stateAsset, HNSFStateAction stateActionAsset)
        {
            var bState = workingState.baseState;
            if (bState == default || !QuantumUnityDB.TryGetGlobalAssetEditorInstance(bState, out var baseState))
                return false;

            bool isIgnored = false;
            while (baseState != null)
            {
                if (baseState.ignoredActions.Exists((x) => x.stateRef == stateAsset && x.actionId == stateActionAsset.id))
                {
                    isIgnored = true;
                    baseState = null;
                    break;
                }

                if (baseState.useBaseState == false)
                {
                    baseState = null;
                    break;
                }

                if (!QuantumUnityDB.TryGetGlobalAssetEditorInstance(baseState.baseState, out baseState))
                {
                    baseState = null;
                    break;
                }
            }
            return isIgnored;
        }

        public static void ValidateStateIDs(HNSFState stateAsset)
        {
            HashSet<int> foundIds = new HashSet<int>();

            for (int i = 0; i < stateAsset.data.Length; i++)
            {
                ValidateActionChildrenRecursive(stateAsset.data[i], ref foundIds);
            }
        }

        private static void ValidateActionChildrenRecursive(HNSFStateAction hnsfStateAction, ref HashSet<int> foundIds)
        {
            if (foundIds.Contains(hnsfStateAction.id))
            {
                Debug.LogError($"Found duplicate ID of {hnsfStateAction.id}");
            }

            foundIds.Add(hnsfStateAction.id);
            
            for(int i = 0; i < hnsfStateAction.children.Length; i++) ValidateActionChildrenRecursive(hnsfStateAction.children[i], ref foundIds);
        }
        
        public static void RegenerateIDs(HNSFStateAction copy, ref int validIdCounter)
        {
            copy.id = validIdCounter;
            for (int i = 0; i < copy.children.Length; i++)
            {
                validIdCounter += 1;
                RegenerateIDs(copy.children[i], ref validIdCounter);
            }
        }
    }
}