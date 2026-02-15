using System;
using Cysharp.Threading.Tasks;
using HnSF.Input;
using HnSF.sessionhandling;
using UnityEngine;
using UnityEngine.Events;
#if HNSF_PEW_EOS
using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using PlayEveryWare.EpicOnlineServices;
#endif
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
        
        public InputManager inputManager;
        public SplitScreenManager splitScreenManager;
        public ModManager modManager;
        public ModContentManager contentManager;
        public AudioListenerManager audioListenerManager;
        public SessionHandlerManager sessionHandlerManager;
        public MusicManager musicManager;
#if HNSF_PEW_EOS
        public EOSManager eosManager;
        public bool createEosManager = true;
#endif
        public bool autoInitialize = true;
        
        
        public void Awake()
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

        public async UniTask Initialize(bool dontDestroyOnLoad = true)
        {
            initialized = false;
            if(dontDestroyOnLoad && transform.parent == null) DontDestroyOnLoad(gameObject);
            instance = this;
            inputManager?.Initialize();
            splitScreenManager?.Init();
            if(modManager != null) await modManager.Init();
            contentManager?.Init();
            contentManager.RegisterAll();
            musicManager.Initialize();
#if HNSF_PEW_EOS
            if (createEosManager && eosManager == null)
            {
                var eosManagerGameobject = new GameObject("EOS Manager", new System.Type[] { typeof(EOSManager) });
                eosManagerGameobject.transform.SetParent(transform);
                eosManager = eosManagerGameobject.GetComponent<EOSManager>();
            }
#endif
            initialized = true;
            WhenInitialized.Invoke();
        }

        private void OnDestroy() 
        {
            if(instance == this) instance = null;
        }
    }
}