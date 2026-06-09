using UnityEngine;

public class ParticleSystemVisualEffect : VisualEffectBase
{
    public override float CurrentTime { get => particleSystems[0].time; protected set => SeekTo(value); }
    
    public ParticleSystem[] particleSystems;

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (!autoStop) return;
        if (destoryTimer > autoStopAfter) Stop(false);
    }

    public override void Play()
    {
        foreach (var ps in particleSystems)
        {
            ps.Stop(true);
            ps.Play(true);
        }
        base.Play();
    }

    public override void Freeze()
    {
        foreach (var ps in particleSystems)
        {
            ps.Pause(true);
        }
        base.Freeze();
    }

    public override void Resume()
    {
        foreach (var ps in particleSystems)
        {
            ps.Play(true);
        }
        base.Resume();
    }

    public override void SeekTo(float time, bool play = true)
    {
        foreach (var ps in particleSystems)
        {
            ps.Simulate(time, true, true);
        }
        if(play) Resume();
        base.SeekTo(time, play);
    }

    public override void Stop(bool clearParticles = true)
    {
        foreach (var ps in particleSystems)
        {
            ps.Stop(true,
                clearParticles ? ParticleSystemStopBehavior.StopEmittingAndClear : ParticleSystemStopBehavior.StopEmitting);
        }
        base.Stop(clearParticles);
    }

    public override void DestroyEffect()
    {
        base.DestroyEffect();
        if (sourcePool != null)
        {
            sourcePool.Release(this);
            return;
        }
        GameObject.Destroy(gameObject);
    }

    public override bool EffectHasStopped()
    {
        foreach (var ps in particleSystems)
        {
            if (ps.particleCount > 0) return false;
        }
        return true;
    }

    public override void SetSeed(uint seed)
    {
        foreach (var ps in particleSystems)
        {
            ps.randomSeed = seed;
        }
    }
    
    protected override void UpdatePlayRate()
    {
        foreach (var ps in particleSystems)
        {
            var main = ps.main;
            main.simulationSpeed = playRate;
        }
    }
}
