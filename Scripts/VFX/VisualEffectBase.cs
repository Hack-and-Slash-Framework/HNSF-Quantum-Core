using System;
using Quantum;
using UnityEngine;
using UnityEngine.Pool;

public class VisualEffectBase : MonoBehaviour
{
    public delegate void EffectDelegate(VisualEffectBase visualEffect);
    public EffectDelegate WhenEffectDestroyed;
    
    public enum VisualEffectPlayStatus
    {
        Stopped,
        Playing,
        Paused
    }
    
    public virtual float CurrentTime { get; protected set; } = 0;

    public virtual float PlayRate
    {
        get
        {
            return playRate;
        }
        set
        {
            if (Mathf.Approximately(playRate, value)) return;
            playRate = value;
            UpdatePlayRate();
        }
    }

    protected float playRate = 1;
    
    public VisualEffectPlayStatus status = VisualEffectPlayStatus.Stopped;
    
    public bool autoDestroy;
    public float autoDestroyAfter = 3.0f;
    public bool autoStop;
    public float autoStopAfter = 1.0f;
    protected float destoryTimer = 0;

    [NonSerialized] public VisualEffectEntry entryAsset;
    public bool freezeDuringHitstop;

    public ObjectPool<VisualEffectBase> sourcePool;
    
    public virtual void FixedUpdate()
    {
        if (!autoDestroy || status == VisualEffectPlayStatus.Paused) return;
        destoryTimer += Time.fixedDeltaTime;
        if(destoryTimer > autoDestroyAfter) DestroyEffect();
    }

    public virtual void Play()
    {
        status = VisualEffectPlayStatus.Playing;
    }

    public virtual void Freeze()
    {
        status = VisualEffectPlayStatus.Paused;
    }

    public virtual void Resume()
    {
        status = VisualEffectPlayStatus.Playing;
    }

    public virtual void SeekTo(float time, bool play = true)
    {
        
    }

    public virtual void Stop(bool clearParticles = true)
    {
        status = VisualEffectPlayStatus.Stopped;
    }

    public virtual void DestroyEffect()
    {
        WhenEffectDestroyed?.Invoke(this);
    }

    public virtual void SetSeed(uint seed)
    {
        
    }
    
    protected virtual void UpdatePlayRate()
    {
        
    }

    public virtual void Reinitialize()
    {
        destoryTimer = 0;
        Stop(true);
    }
}
