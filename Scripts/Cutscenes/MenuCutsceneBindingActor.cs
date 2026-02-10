using UnityEngine;
using UnityEngine.Playables;

namespace HnSF
{
    public class MenuCutsceneBindingActor : MonoBehaviour
    {
        public BehaviourCutsceneBindingSource bindingSource;
        public PlayableDirector director;
        public CutsceneBinder binder;
    }
}
