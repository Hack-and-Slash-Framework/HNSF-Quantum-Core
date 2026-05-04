#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HnSF
{
    public static class EditorDeltaTime
    {
        public static double editorDeltaTime = 0f;
        public static double lastTimeSinceStartup = 0f;

        public static void Reset()
        {
            editorDeltaTime = 0;
            lastTimeSinceStartup = 0;
        }
        
        public static void SetEditorDeltaTime()
        {
            #if UNITY_EDITOR
            if (lastTimeSinceStartup == 0f)
            {
                lastTimeSinceStartup = EditorApplication.timeSinceStartup;
            }
            editorDeltaTime = EditorApplication.timeSinceStartup - lastTimeSinceStartup;
            lastTimeSinceStartup = EditorApplication.timeSinceStartup;
            #endif
        }
    }
}