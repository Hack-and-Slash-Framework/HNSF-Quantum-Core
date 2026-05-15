using System.Collections.Generic;
using Quantum;
using UnityEngine;
using UnityEngine.Serialization;

namespace HnSF
{
    public class CutsceneGrouping : MonoBehaviour
    {
        public Dictionary<AssetRef<Tag>, ActorCutscenePlayer> CutscenePlayersMap = new();

        public AssetRef sourceKey;
        [FormerlySerializedAs("_cutscenePlayers")]
        public List<ActorCutscenePlayer> cutscenePlayers = new();
        public CutsceneBindingSource bindingSource;
        
        protected virtual void Awake()
        {
            BuildMap();
        }

        public virtual void BuildMap()
        {
            CutscenePlayersMap.Clear();
            foreach(var gcp in cutscenePlayers) CutscenePlayersMap.Add(gcp.cutscenePlayerTag, gcp);
        }

        public void StopAll(bool pause = false)
        {
            foreach (var gcp in cutscenePlayers)
            {
                gcp.StopCutscene(pause);
            }
        }
    }
}