using System.Collections.Generic;
using Quantum;
using UnityEngine;

namespace HnSF
{
    public class CutsceneGrouping : MonoBehaviour
    {
        public Dictionary<AssetRef<Tag>, ActorCutscenePlayer> cutscenePlayers = new();
        
        [SerializeField] private List<ActorCutscenePlayer> _cutscenePlayers = new();

        public CutsceneBindingSource bindingSource;
        
        private void Awake()
        {
            foreach(var gcp in _cutscenePlayers) cutscenePlayers.Add(gcp.cutscenePlayerTag, gcp);
        }

        public void StopAll()
        {
            foreach (var gcp in _cutscenePlayers)
            {
                gcp.StopCutscene();
            }
        }
    }
}