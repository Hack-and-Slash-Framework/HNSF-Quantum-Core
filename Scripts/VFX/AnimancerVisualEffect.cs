using System.Collections;
using System.Collections.Generic;
#if HNSF_ANIMANCER
using Animancer;
#endif
using UnityEngine;

public class AnimancerVisualEffect : VisualEffectBase
{
#if HNSF_ANIMANCER
    public override float CurrentTime { get => animancer.Layers[0].GetOrCreateState(clip).Time; protected set => animancer.Layers[0].GetOrCreateState(clip).Time = value; }
    
    public AnimancerComponent animancer;
    public AnimationClip clip;

    private AnimancerState currentState;
    
    public override void Play()
    {
        currentState = animancer.Play(clip);
        base.Play();
    }

    public override void Freeze()
    {
        animancer.Graph.PauseGraph();
        base.Freeze();
    }

    public override void Resume()
    {
        animancer.Graph.UnpauseGraph();
        base.Resume();
    }

    public override void SeekTo(float time, bool play = true)
    {
        currentState.Time = time;
        Freeze();
        if(play) Resume();
        base.SeekTo(time, play);
    }

    public override void Stop(bool clearParticles = true)
    {
        animancer.Stop(clip);
        base.Stop(clearParticles);
    }

    public override void DestroyEffect()
    {
        if (sourcePool != null)
        {
            sourcePool.Release(this);
            return;
        }
        GameObject.Destroy(gameObject);
    }
    
    protected override void UpdatePlayRate()
    {
        animancer.Graph.Speed = playRate;
    }
#endif
}
