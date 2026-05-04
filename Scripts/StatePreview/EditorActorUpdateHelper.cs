using UnityEngine;

namespace HnSF
{
    public static class EditorActorUpdateHelper
    {
        public static void DoAwake(GameObject go)
        {
            /*
            foreach (var com in go.GetComponents<IEditorAwake>())
            {
                com.Awake();
            }*/
            
            foreach (var com in go.GetComponentsInChildren<IEditorAwake>())
            {
                com.Awake();
            }

            foreach (var com in go.GetComponents<IEditorOnEnable>())
            {
                com.OnEnable();
            }
        }

        public static void DoDisable(GameObject go)
        {
            foreach (var com in go.GetComponents<IEditorOnDisable>())
            {
                com.OnDisable();
            }
        }
    }
}