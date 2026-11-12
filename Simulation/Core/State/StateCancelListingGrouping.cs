using System;
using System.Collections.Generic;
using HnSF.core.state;
using HnSF.core.state.decisions;
#if QUANTUM_UNITY
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
#endif

namespace Quantum
{
    public class StateCancelListingGrouping : StateCancelListing
    {
#if UNITY_EDITOR
        [SerializeField] private List<AssetRef<StateCancelListing>> otherListings = new List<AssetRef<StateCancelListing>>();
#endif
        
        private void OnValidate()
        {
            #if UNITY_EDITOR
            if (Application.isPlaying)
                return;
            
            entries.Clear();

            for (int i = 0; i < otherListings.Count; i++)
            {
                if (!QuantumUnityDB.TryGetGlobalAssetEditorInstance(otherListings[i], out var scl))
                {
                    continue;
                }

                foreach (var e in scl.entries)
                {
                    entries.Add(e);
                }
            }
            
            EditorUtility.SetDirty(this);
            #endif
        }
    }
}
