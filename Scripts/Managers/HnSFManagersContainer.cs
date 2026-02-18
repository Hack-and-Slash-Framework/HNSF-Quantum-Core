using System;
using Cysharp.Threading.Tasks;
using HnSF.sessionhandling;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HnSF
{
    public class HnSFManagersContainer : MonoBehaviour
    {
        [NonSerialized] public static UnityEvent WhenInitialized = new UnityEvent();
        [NonSerialized] public static bool initialized = false;
        
        public static HnSFManagersContainer instance = null;
        
        public ModManager modManager;
        public ModContentManager contentManager;
        public SessionHandlerManager sessionHandlerManager;
        public bool autoInitialize = true;
        
        
        protected virtual void Awake()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnExitPlayMode;
#endif
            if (autoInitialize == false) return;
            if (instance != null)
            {
                GameObject.Destroy(gameObject);
                return;
            }
            _ = Initialize();
        }
        
#if UNITY_EDITOR
        private static void OnExitPlayMode(PlayModeStateChange state)
        {
            if(state == PlayModeStateChange.ExitingPlayMode)
            {
                EditorApplication.playModeStateChanged -= OnExitPlayMode;
                instance = null;
                initialized = false;
                WhenInitialized.RemoveAllListeners();
            }
        }
#endif

        public virtual async UniTask Initialize(bool dontDestroyOnLoad = true)
        {
            initialized = false;
            if(dontDestroyOnLoad && transform.parent == null) DontDestroyOnLoad(gameObject);
            instance = this;
            await InitializeManagers();
            initialized = true;
            WhenInitialized.Invoke();
        }

        protected virtual async UniTask InitializeManagers()
        {
            if(modManager != null) await modManager.Init();
            contentManager?.Init();
            contentManager.RegisterAll();
        }

        protected virtual void OnDestroy() 
        {
            if(instance == this) instance = null;
        }
    }
}