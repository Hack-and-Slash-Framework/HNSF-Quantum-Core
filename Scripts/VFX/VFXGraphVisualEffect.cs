using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VFXGraphVisualEffect : VisualEffectBase
{
    public VisualEffect[] visualEffects;

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
            ve.pause = true;
        }
        base.Freeze();
    }

    public override void Resume()
    {
        foreach (var ve in visualEffects)
        {
            ve.pause = false;
        }
        base.Resume();
    }

    public override void SeekTo(float time, bool play = true)
    {
        foreach (var ve in visualEffects)
        {
            ve.Reinit();
            ve.pause = true;
            ve.Simulate(time);
        }
        if(play) Resume();
        base.SeekTo(time, play);
    }

    public override void Stop(bool clearParticles = true)
    {
        foreach (var ve in visualEffects)
        {
            ve.Stop();
        }
        base.Stop(clearParticles);
    }

    public override void DestroyEffect()
    {
        Destroy(gameObject);
    }
}
