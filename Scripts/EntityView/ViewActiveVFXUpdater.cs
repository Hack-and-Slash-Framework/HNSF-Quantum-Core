using System;
using System.Collections.Generic;
using Quantum;
using UnityEngine;

namespace HnSF
{
    public unsafe class ViewActiveVFXUpdater : QuantumEntityViewComponent
    {
        public class ViewVFXRepresentation
        {
            public bool found;
            public ActiveVFXPart simRepresentation;
            public VisualEffectBase visualEffect;
        }
        
        public QuantumEntityView entityView;

        public Dictionary<int, ViewVFXRepresentation> vfxs = new();
        public List<VisualEffectBase> stoppingVisualEffects = new();
        
        [NonSerialized] protected List<int> entriesToRemove = new List<int>();

        protected virtual void OnDisable()
        {
            foreach (var vfx in vfxs)
            {
                if(vfx.Value.visualEffect != null) GameObject.Destroy(vfx.Value.visualEffect.gameObject);
            }

            foreach (var stoppingVisualEffect in stoppingVisualEffects)
            {
                GameObject.Destroy(stoppingVisualEffect?.gameObject);
            }
            
            vfxs.Clear();
            stoppingVisualEffects.Clear();
            entriesToRemove.Clear();
        }
        
        public override void OnUpdateView()
        {
            var frame = Game.Frames.Predicted;
            if (!frame.Unsafe.TryGetPointer<ActiveVFXContainer>(entityView.EntityRef, out var container)) return;
            if(!frame.TryResolveList(container->vfxParts, out var vfxList)) return;
            
            // Clear Found Flag
            foreach (var vfx in vfxs) vfx.Value.found = false;

            // Update View List
            foreach (var svfx in vfxList)
            {
                // Already Found
                if (vfxs.ContainsKey(svfx.GetHashCode()))
                {
                    vfxs[svfx.GetHashCode()].found = true;
                    continue;
                }
                
                // New, add to list
                vfxs.Add(svfx.GetHashCode(), new ViewVFXRepresentation()
                {
                    found = true,
                    simRepresentation = svfx,
                    visualEffect = null
                });
            }
            
            // Create/Destroy VFX
            foreach (var vfx in vfxs)
            {
                // Stop Effect
                if (vfx.Value.found == false)
                {
                    if (vfx.Value.visualEffect != null)
                    {
                        stoppingVisualEffects.Add(vfx.Value.visualEffect);
                    }
                    entriesToRemove.Add(vfx.Key);
                    continue;
                }

                // Create Effect
                if (vfx.Value.visualEffect == null 
                    && QuantumUnityDB.TryGetGlobalAsset(vfx.Value.simRepresentation.vfx, out var vfxAsset) 
                    && vfxAsset.visualEffect != null)
                {
                    var ve = GameObject.Instantiate(vfxAsset.visualEffect.GetComponent<VisualEffectBase>(), transform, false);
                    ve.transform.localPosition = vfx.Value.simRepresentation.offset.ToUnityVector3();
                    ve.SeekTo((float)(frame.Number - vfx.Value.simRepresentation.playFrame) / (60.0f), true);
                    vfx.Value.visualEffect = ve;
                }
            }
            
            // Remove Expired VFX entries
            foreach(var vfxEntryKey in entriesToRemove)
                vfxs.Remove(vfxEntryKey);
            entriesToRemove.Clear();
            
            // Check Stopping VFXs
            for (int i = stoppingVisualEffects.Count - 1; i >= 0; i--)
            {
                if(!stoppingVisualEffects[i].EffectHasStopped()) continue;
                GameObject.Destroy(stoppingVisualEffects[i].gameObject);
                stoppingVisualEffects.RemoveAt(i);
            }
        }
    }
}