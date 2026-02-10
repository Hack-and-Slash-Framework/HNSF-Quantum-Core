using System.Collections.Generic;
using Quantum;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace HnSF
{
    public class CutsceneBinder : MonoBehaviour, ITimelineDirectorBinder
    {
        public PlayableDirector director;
        public TimelineAssetOverride bindings;
        [SerializeReference, SubclassSelector]
        public List<ITimelineExposedReferenceData> exposedReferences = new();
        
        public virtual void Bind()
        {
        }
        
        public virtual void Bind(ITimelineDirectorBindingSource bindingSource)
        {
            bindings.SetOverridesOnly(director, bindingSource);
            
            for (int i = 0; i < exposedReferences.Count; i++)
            {
                if (exposedReferences[i].GetType() == typeof(TimelineExposedReferenceDataFromBindingSource))
                {
                    var tag = exposedReferences[i].GetReference() as Tag;
                    director.SetReferenceValue(exposedReferences[i].GetID(), bindingSource.GetMapping(tag));
                }
                else
                {
                    director.SetReferenceValue(exposedReferences[i].GetID(), exposedReferences[i].GetReference());
                }
            }

            
            // Update bindings for any timelines that this one plays.
            foreach (var pao in director.playableAsset.outputs)
            {
                if(!pao.streamName.Contains("Control Track")) continue;

                var controlTrack = pao.sourceObject as ControlTrack;

                foreach (var clip in controlTrack.GetClips())
                {
                    var c = clip.asset as ControlPlayableAsset;
                    if(c == null) continue;
                    
                    var directorGameObject = c.sourceGameObject.Resolve(director);
                    if (directorGameObject == null)
                        continue;

                    if (directorGameObject.TryGetComponent<CutsceneBinder>(out var bcb))
                    {
                        bcb.Bind(bindingSource);
                    }
                }
            }
        }

        public virtual void Bind(QuantumGame qGame)
        {
        }

        public virtual void Bind(QuantumGame qGame, ITimelineDirectorBindingSource bindingSource)
        {
            Bind(bindingSource);
        }
    }
}