#if HNSF_ANIMANCER
using Animancer;
#endif
using UnityEngine;

public class AnimancerSoloAnimationVisualEffect : VisualEffectBase
{
#if HNSF_ANIMANCER
    public override float CurrentTime { get => animancer.Time; protected set => animancer.Time = value; }
    
    public SoloAnimation animancer;
    public AnimationClip clip;
    
    public override void Play()
    {
        animancer.Play(clip);
        base.Play();
    }

    public override void Freeze()
    {
        animancer.IsPlaying = false;
        base.Freeze();
    }

    public override void Resume()
    {
        animancer.IsPlaying = true;
        base.Resume();
    }

    public override void SeekTo(float time, bool play = true)
    {
        animancer.Time = time;
        Freeze();
        if(play) Resume();
        base.SeekTo(time, play);
    }

    public override void Stop(bool clearParticles = true)
    {
        animancer.Time = clip.length;
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
        animancer.Speed = playRate;
    }
#endif
}