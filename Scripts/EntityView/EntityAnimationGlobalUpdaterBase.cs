using System.Collections.Generic;
using Quantum;
using UnityEngine.Profiling;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HnSF
{
    public class EntityAnimationGlobalUpdaterBase : QuantumSceneViewComponent
    {
        protected static List<EntityAnimationUpdaterBase> eauList = new List<EntityAnimationUpdaterBase>();
        protected static HashSet<EntityAnimationUpdaterBase> priorityEau = new HashSet<EntityAnimationUpdaterBase>();

        private void Awake()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnExitPlayMode;
#endif
        }

#if UNITY_EDITOR
        private static void OnExitPlayMode(PlayModeStateChange state)
        {
            if(state == PlayModeStateChange.ExitingPlayMode)
            {
                EditorApplication.playModeStateChanged -= OnExitPlayMode;
                eauList = new List<EntityAnimationUpdaterBase>();
                priorityEau = new HashSet<EntityAnimationUpdaterBase>();
            }
        }
#endif 
        
        public override void OnActivate(Frame frame)
        {
            base.OnActivate(frame);
        }
        
        public override void OnEnable()
        {
            base.OnEnable();
        }
        
        public override void OnUpdateView()
        {
            Profiler.BeginSample("Animation Global Updater");
            foreach (var pEau in priorityEau)
            {
                pEau.UpdateAnimatorState(Game);
            }
            for (var index = 0; index < eauList.Count; index++)
            {
                eauList[index].UpdateAnimatorState(Game);
            }
            Profiler.EndSample();
        }
        
        public static void RegisterAnimator(EntityAnimationUpdaterBase eau, bool priorityAnimator = false)
        {
            if (priorityAnimator)
            {
                priorityEau.Add(eau);
            }
            else
            {
                eauList.Add(eau);
            }
        }
        
        public static void UnregisterAnimator(EntityAnimationUpdaterBase eau)
        {
            if (!priorityEau.Remove(eau))
            {
                eauList.Remove(eau);
            }
        }
    }
}