using System.Collections.Generic;
using HnSF;
using Quantum;
using UnityEngine;

public unsafe class FighterVFXManager : MonoBehaviour, IEditorOnEnable, IEditorOnDisable
{
    public QuantumEntityView view;
    
    public List<VisualEffectBase> currentPlayingEffects = new();
    private DispatcherSubscription _updateViewDispatcher;

    public virtual void OnEnable()
    {
        _updateViewDispatcher = QuantumCallback.Subscribe(this, (CallbackUpdateView callback) => UpdateView(callback));
    }

    public virtual void OnDisable()
    {
        QuantumCallback.Unsubscribe(_updateViewDispatcher);
    }
    
    private void UpdateView(CallbackUpdateView callback)
    {
        var game = callback.Game;
        
        var hasHitstop = game.Frames.Predicted.Unsafe.TryGetPointer<Hitstop>(view.EntityRef, out var hitstop);
        var hasLdt = game.Frames.Predicted.Unsafe.TryGetPointer<LocalDeltaTime>(view.EntityRef, out var ldt);
        
        for (int i = currentPlayingEffects.Count-1; i >= 0; i--)
        {
            if (currentPlayingEffects[i] == null)
            {
                currentPlayingEffects.RemoveAt(i);
                continue;
            }

            if (hasLdt) currentPlayingEffects[i].PlayRate = ldt->multiplier.AsFloat;
            
            if (hasHitstop 
                && hitstop->value > 0
                && currentPlayingEffects[i].freezeDuringHitstop
                && currentPlayingEffects[i].status == VisualEffectBase.VisualEffectPlayStatus.Playing)
            {
                currentPlayingEffects[i].Freeze();
            }else if (hasHitstop 
                      && hitstop->value <= 0
                      && currentPlayingEffects[i].freezeDuringHitstop
                      && currentPlayingEffects[i].status == VisualEffectBase.VisualEffectPlayStatus.Paused)
            {
                currentPlayingEffects[i].Resume();
            }
        }
    }

    public virtual void Play(VisualEffectEntry effectEntry, VisualEffectBase visualEffect)
    {
        visualEffect.WhenEffectDestroyed += WhenVisualEffectDestroyed;
        visualEffect.Play();
        currentPlayingEffects.Add(visualEffect);
    }

    public virtual void Destroy(VisualEffectBase visualEffect)
    {
        
    }
    
    private void WhenVisualEffectDestroyed(VisualEffectBase visualeffect)
    {
        currentPlayingEffects.Remove(visualeffect);
    }

    public virtual void StopEffect(AssetRef<VisualEffectEntry> effectRef, int offset, bool destroyParticles, bool unparent)
    {
        if (currentPlayingEffects.Count == 0) return;
        int currentCount = 0;
        VisualEffectBase effectToStop = null;
        for (int i = currentPlayingEffects.Count - 1; i >= 0; i--)
        {
            if (currentPlayingEffects[i].entryAsset.Identifier.Guid != effectRef.Id || currentPlayingEffects[i].status == VisualEffectBase.VisualEffectPlayStatus.Stopped) continue;
            currentCount++;
            if (currentCount <= offset) continue;
            effectToStop = currentPlayingEffects[i];
            break;
        }
        if (effectToStop == null) return;
        
        effectToStop.Stop(destroyParticles);
        if(unparent) effectToStop.transform.SetParent(null, true);
    }
    
    public virtual void StopAllEffectsOfType(AssetRef<VisualEffectEntry> effectRef, bool destroyParticles)
    {
        if (currentPlayingEffects.Count == 0) return;
        for (int i = 0; i < currentPlayingEffects.Count; i++)
        {
            if (currentPlayingEffects[i].entryAsset!= effectRef) continue;
            currentPlayingEffects[i].Stop(destroyParticles);
        }
    }
    
    public virtual void StopLastEffect(bool destroyParticles)
    {
        if (currentPlayingEffects.Count == 0) return;
        currentPlayingEffects[^1].Stop(destroyParticles);
    }
    
    private void StopAllEffects()
    {
        for (int i = currentPlayingEffects.Count - 1; i >= 0; i--)
        {
            currentPlayingEffects[i].DestroyEffect();
        }
        currentPlayingEffects.Clear();
    }
}
