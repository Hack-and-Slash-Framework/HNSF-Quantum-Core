using Quantum;
using Unity.Cinemachine;
using UnityEngine;

namespace HnSF
{
    public class VirtualCameraCutsceneTagAssigning : MonoBehaviour, ICutsceneBinding
    {
        public CinemachineCamera virtualCamera;

        public AssetRef<Tag> followTag;
        public AssetRef<Tag> lookAtTag;

        public void Bind(QuantumGame qGame, CutsceneBindingSource bindingSource)
        {
            var follow = bindingSource.GetMapping(followTag) as GameObject;
            var lookAt = bindingSource.GetMapping(lookAtTag) as GameObject;

            if (follow) virtualCamera.Follow = follow.transform;
            if (lookAt) virtualCamera.LookAt = lookAt.transform;
        }
    }
}