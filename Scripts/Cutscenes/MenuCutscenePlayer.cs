using System;
using Cysharp.Threading.Tasks;
using UnityEngine.Playables;
using UnityEngine.Serialization;

[System.Serializable]
public class MenuCutscenePlayer
{
    public struct CutscenePlayerDirectorDefinition
    {
        public PlayableDirector director;
        public bool reportedEndOfCutscene;
    }
    
    private bool autoCleanup;
    private bool reportedEndOfCutscene;
    public delegate void DelegateCutscenePlayer(MenuCutscenePlayer cutscenePlayer);
    public delegate void DelegateCutscenePlayerIndex(MenuCutscenePlayer cutscenePlayer, int index);
    public DelegateCutscenePlayer eventOnCutsceneEnded;
    public DelegateCutscenePlayerIndex eventOnCutscenePieceEnded;
    
    public CutscenePlayerDirectorDefinition[] directorDefinitions;
    
    public void Play(PlayableDirector[] director, bool autoCleanup = false)
    {
        if (directorDefinitions is { Length: > 0 }) SkipToEndAndCleanup();
        this.autoCleanup = autoCleanup;

        reportedEndOfCutscene = false;
        directorDefinitions = new CutscenePlayerDirectorDefinition[director.Length];
        for (int i = 0; i < directorDefinitions.Length; i++)
        {
            directorDefinitions[i].director = director[i];
            directorDefinitions[i].director.Play();
        }
        _ = CallEndOfCutsceneEvent();
    }

    private async UniTask CallEndOfCutsceneEvent()
    {
        if (directorDefinitions == null) return;
        await UniTask.WaitUntil(AreDirectorsFinished);
        if (reportedEndOfCutscene) return;
        eventOnCutsceneEnded?.Invoke(this);
        if(autoCleanup) Cleanup();
        reportedEndOfCutscene = true;
    }
    
    public async UniTask WaitForEndOfCutscene()
    {
        if (directorDefinitions == null) return;
        await UniTask.WaitUntil(AreDirectorsFinished);
    }

    public void SkipToEnd()
    {
        if (directorDefinitions == null || directorDefinitions.Length == 0) return;
        for (int i = 0; i < directorDefinitions.Length; i++)
        {
            directorDefinitions[i].director.time = directorDefinitions[i].director.playableAsset.duration;
            directorDefinitions[i].director.Evaluate();
            if (!directorDefinitions[i].reportedEndOfCutscene)
            {
                // Report end in order of shortest > longest cutscene piece
            }
        }
        ReportEndOfCutscene();
    }
    
    public void SkipToEndAndCleanup()
    {
        if (directorDefinitions == null) return;
        SkipToEnd();
        Cleanup();
    }

    public void Cleanup()
    {
        if (directorDefinitions == null) return;
        for (int i = 0; i < directorDefinitions.Length; i++)
        {
            if(directorDefinitions[i].director != null) directorDefinitions[i].director.Stop();
            directorDefinitions[i].director = null;
            directorDefinitions[i].reportedEndOfCutscene = false;
        }

        directorDefinitions = Array.Empty<CutscenePlayerDirectorDefinition>();
    }

    private bool AreDirectorsFinished()
    {
        for (int i = 0; i < directorDefinitions.Length; i++)
        {
            if (directorDefinitions[i].director.time < directorDefinitions[i].director.duration) return false;
        }
        return true;
    }
    
    public bool IsValid()
    {
        return directorDefinitions is { Length: > 0 } && directorDefinitions[0].director.playableGraph.IsValid();
    }

    private void ReportEndOfCutscenePart(int part)
    {
        directorDefinitions[part].reportedEndOfCutscene = true;
        eventOnCutscenePieceEnded?.Invoke(this, part);
    }
    
    private void ReportEndOfCutscene()
    {
        eventOnCutsceneEnded?.Invoke(this);
        /*
        if(index == -1) eventOnCutsceneEnded?.Invoke(this);
        else
        {
            directorDefinitions[index].reportedEndOfCutscene = true;
            eventOnCutscenePieceEnded?.Invoke(this, index);
        }*/
    }
}
