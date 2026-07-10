using UnityEditor;
using UnityEngine;

namespace HnSF
{
    public static class ClipImportSettingsHelper
    {
        public static bool TryGetClipRotationOffsetY(this AnimationClip clip, out float eulerOffsetY)
        {
            eulerOffsetY = 0f;

            if (clip == null)
                return false;

            string path = AssetDatabase.GetAssetPath(clip);
            if (string.IsNullOrEmpty(path))
                return false;

            if (AssetImporter.GetAtPath(path) is not ModelImporter importer)
                return false;

            var clips = importer.clipAnimations;
            if (clips == null || clips.Length == 0)
                clips = importer.defaultClipAnimations;

            foreach (var clipSettings in clips)
            {
                if (clipSettings.name != clip.name)
                    continue;

                eulerOffsetY = clipSettings.rotationOffset;
                return true;
            }

            return false;
        }
        
        public static bool TryGetClipOffsetY(this AnimationClip clip, out float offsetY)
        {
            offsetY = 0f;

            if (clip == null)
                return false;

            string path = AssetDatabase.GetAssetPath(clip);
            if (string.IsNullOrEmpty(path))
                return false;

            if (AssetImporter.GetAtPath(path) is not ModelImporter importer)
                return false;

            var clips = importer.clipAnimations;
            if (clips == null || clips.Length == 0)
                clips = importer.defaultClipAnimations;

            foreach (var clipSettings in clips)
            {
                if (clipSettings.name != clip.name)
                    continue;

                offsetY = clipSettings.heightOffset;
                return true;
            }

            return false;
        }
        
        public static bool TryGetStandaloneClipOrientationOffsetY(this AnimationClip clip, out float eulerOffsetY)
        {
            eulerOffsetY = 0f;

            if (clip == null)
                return false;

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            eulerOffsetY = settings.orientationOffsetY;
            return true;
        }
        
        public static bool TryGetStandaloneClipOffsetY(this AnimationClip clip, out float offsetY)
        {
            offsetY = 0f;

            if (clip == null)
                return false;

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            offsetY = settings.level;
            return true;
        }
    }
}