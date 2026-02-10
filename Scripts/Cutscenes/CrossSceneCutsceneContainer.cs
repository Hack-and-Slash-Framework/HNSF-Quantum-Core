using Quantum;
using HnSF;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

[System.Serializable]
public class CrossSceneCutsceneContainer : MonoBehaviour
{
    [System.Serializable]
    public struct CutsceneData
    {
        public Tag tag;
        public PlayableDirector director;
    }
    
    public BehaviourCutsceneBindingSource bindingSource;
    public GameObject sceneContainer;
    public Tag containerTag;
    public string sceneName;
    
    public CutsceneData[] cutscenes;

    public void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    public void ShowScene()
    {
        if(sceneContainer) sceneContainer.SetActive(true);
    }
    
    public void HideScene()
    {
        if(sceneContainer) sceneContainer.SetActive(false);
    }

    public void SetSceneAsActive()
    {
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
    }

    public PlayableDirector GetCutsceneDirector(Tag cutsceneTag)
    {
        for (var i = 0; i < cutscenes.Length; i++)
        {
            if (cutscenes[i].tag == cutsceneTag)
            {
                return cutscenes[i].director;
            }
        }
        return null;
    }

    private void OnDestroy()
    {
    }
}
