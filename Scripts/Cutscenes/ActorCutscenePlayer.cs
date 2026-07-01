using System;
using System.Collections.Generic;
using Quantum;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

namespace HnSF
{
    public unsafe class ActorCutscenePlayer : MonoBehaviour
    {
        public virtual double CutsceneTime
        {
            get => director.playableGraph.IsValid() ? director.time : -1;
            set
            {
                director.time = value;
                if (value >= director.duration)
                {
                    switch (director.extrapolationMode)
                    {
                        case DirectorWrapMode.Hold:
                            director.time = director.duration;
                            break;
                        case DirectorWrapMode.Loop:
                            director.time -= director.duration;
                            break;
                    }
                }
            }
        }

        public Action<ActorCutscenePlayer> OnCutsceneReachedEnding;
        
        public List<GameObject> cutsceneBindingsGetters = new();
        
        public AssetRef<Tag> cutscenePlayerTag;
        public CutsceneBinder cutsceneBinder;
        public PlayableDirector director;

        protected bool shouldCallEndEvent = false;
        public bool autoUpdatePlayRate = true;
        public bool takeExclusiveControl = false;
        
        //public QuantumEntityView playingEntityView;
        [NonSerialized] public QuantumGame game;
        [NonSerialized] public EntityRef sourceEntityRef;
        
        public GameObject[] objectsDisabledOnCutsceneEnd;

        public CinemachineVirtualCameraBase[] virtualCameras;
        
        protected virtual void Awake()
        {
            foreach(var cc in cutsceneBindingsGetters) cc.SetActive(false);
        }

        public virtual void PlayCutscene(QuantumGame qGame, CutsceneBindingSource bindingSource, DirectorUpdateMode updateMode = DirectorUpdateMode.GameTime)
        {
            cutsceneBinder.Bind(qGame, bindingSource);
            
            foreach (var cbg in cutsceneBindingsGetters)
            {
                cbg.SetActive(true);
                cbg.GetComponent<ICutsceneBinding>().Bind(qGame, bindingSource);
            }

            if (director.playableGraph.IsValid()) director.time = 0;
            director.Play();
            shouldCallEndEvent = true;
            director.timeUpdateMode = updateMode;
        }

        protected virtual void FixedUpdate()
        {
            if (!shouldCallEndEvent) return;
            
            if (director.state == PlayState.Playing)
            {
                if (autoUpdatePlayRate && sourceEntityRef != EntityRef.None)
                {
                    var frame = game.Frames.Predicted;
                    
                    var hasLdt = frame.Unsafe.TryGetPointer<LocalDeltaTime>(sourceEntityRef, out var ldt);
                    
                    if(hasLdt) SetTimeScale(ldt->multiplier.AsFloat);
                }
                
                if (director.time >= director.playableAsset.duration)
                {
                    OnCutsceneReachedEnding?.Invoke(this);
                    shouldCallEndEvent = false;
                }
            }
        }

        public virtual void StopCutscene(bool pause = false, bool setTimeToEnd = true)
        {
            if (pause)
            {
                if(director.playableAsset != null && setTimeToEnd)
                {
                    director.time = director.playableAsset.duration;
                }
                director.Pause();
            }
            else
            {
                if (director.playableAsset != null && setTimeToEnd)
                {
                    director.time = director.playableAsset.duration;
                    director.Evaluate();
                }
                director.Stop();
            }
            shouldCallEndEvent = false;
            
            foreach(var go in objectsDisabledOnCutsceneEnd) go.SetActive(false);
        }

        public virtual void PostTimelineTicked()
        {
            
        }

        public virtual double GetCutsceneLength()
        {
            return director == null || director.playableAsset == null ? 0 : director.playableAsset.duration;
        }
        
        public virtual void SetTimeScale(float playRate)
        {
            if(!director.playableGraph.IsValid()) director.RebuildGraph();
            director.playableGraph.GetRootPlayable(0).SetSpeed(playRate);
        }

        public virtual void AssignVirtualCameraLayer(OutputChannels cameraOutputChannels)
        {
            foreach (var vc in virtualCameras)
            {
                vc.OutputChannel = cameraOutputChannels;
            }
        }
    }
}