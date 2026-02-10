using System.Collections.Generic;
using UnityEngine;

namespace HnSF
{
    public static class LocalCameraRepo
    {
        public static HashSet<Camera> localCameras = new HashSet<Camera>();
        
        public static float GetClosestDistanceFromCameras(Vector3 position)
        {
            float furthestDistance = float.MaxValue;
            foreach (var localCam in LocalCameraRepo.localCameras)
            {
                furthestDistance = Mathf.Min(furthestDistance, (position - localCam.transform.position).magnitude);
            }
            return furthestDistance;
        }
        
        public static float GetClosestSqrDistanceFromCameras(Vector3 position)
        {
            float furthestDistance = float.MaxValue;
            foreach (var localCam in LocalCameraRepo.localCameras)
            {
                furthestDistance = Mathf.Min(furthestDistance, (position - localCam.transform.position).sqrMagnitude);
            }
            return furthestDistance;
        }
        
        public static float GetFurthestDistanceFromCameras(Vector3 position)
        {
            float furthestDistance = 0;
            foreach (var localCam in LocalCameraRepo.localCameras)
            {
                furthestDistance = Mathf.Max(furthestDistance, (position - localCam.transform.position).magnitude);
            }
            return furthestDistance;
        }
        
        public static float GetFurthestSqrDistanceFromCameras(Vector3 position)
        {
            float furthestDistance = 0;
            foreach (var localCam in LocalCameraRepo.localCameras)
            {
                furthestDistance = Mathf.Max(furthestDistance, (position - localCam.transform.position).sqrMagnitude);
            }
            return furthestDistance;
        }
        
        public static bool PositionWithinCamerasView(Vector3 unityPosition, float xPositionBias = 0.0f, float yPositionBias = 0.0f, float zPositionBias = 0.0f)
        {
            foreach (var localCam in LocalCameraRepo.localCameras)
            {
                var normalizedPos = localCam.WorldToViewportPoint(unityPosition);
                if (normalizedPos.z <= (0 - zPositionBias)) continue;
                if(normalizedPos.y < (0 - yPositionBias) || normalizedPos.y > (1 + yPositionBias)) continue;
                if(normalizedPos.x < (0 - xPositionBias) || normalizedPos.x > (1 + xPositionBias)) continue;
                return true;
            }
            return false;
        }
        
        public static bool PositionWithinCamerasView2D(Vector3 unityPosition, float xPositionBias = 0.0f, float yPositionBias = 0.0f)
        {
            foreach (var localCam in LocalCameraRepo.localCameras)
            {
                var normalizedPos = localCam.WorldToViewportPoint(unityPosition);
                if(normalizedPos.y < (0 - yPositionBias) || normalizedPos.y > (1 + yPositionBias)) continue;
                if(normalizedPos.x < (0 - xPositionBias) || normalizedPos.x > (1 + xPositionBias)) continue;
                return true;
            }
            return false;
        }
    }
}