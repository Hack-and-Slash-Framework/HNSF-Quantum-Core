using UnityEngine;

namespace HnSF
{
    public class LocalCameraRegistrator : MonoBehaviour
    {
        [SerializeField] protected Camera cam;
        
        private void OnEnable()
        {
            LocalCameraRepo.localCameras.Add(cam);
        }

        private void OnDisable()
        {
            LocalCameraRepo.localCameras.Remove(cam);
        }
    }
}