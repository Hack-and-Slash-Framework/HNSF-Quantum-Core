using UnityEngine;

public class GroupVisualEffect : VisualEffectBase
{
    public VisualEffectBase[] visualEffects;

    public override void Play()
    {
        foreach (var ve in visualEffects)
        {
            ve.Play();
        }
        base.Play();
    }

    public override void Freeze()
    {
        foreach (var ve in visualEffects)
        {
            ve.Freeze();
        }
        base.Freeze();
    }

    public override void Resume()
    {
        foreach (var ve in visualEffects)
        {
            ve.Resume();
        }
        base.Resume();
    }

    public override void SeekTo(float time, bool play = true)
    {
        foreach (var ve in visualEffects)
        {
            ve.SeekTo(time, play);
        }
        base.SeekTo(time, play);
    }

    public override void Stop(bool clearParticles = true)
    {
        foreach (var ve in visualEffects)
        {
            ve.Stop();
        }
        base.Stop();
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

    public override void SetSeed(uint seed)
    {
        foreach (var ve in visualEffects)
        {
            ve.SetSeed(seed);
        }
    }
    protected override void UpdatePlayRate()
    {
        foreach (var ve in visualEffects)
        {
            ve.PlayRate = playRate;
        }
    }
}
