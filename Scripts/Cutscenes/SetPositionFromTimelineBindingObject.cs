using Quantum;
using UnityEngine;
using UnityEngine.Serialization;

namespace HnSF
{
    public class SetPositionFromTimelineBindingObject : MonoBehaviour, ITimelineDirectorBindingGetter
    {
        public GameObject followTarget;
        [FormerlySerializedAs("tag")] public AssetRef<Tag> targetTag;
        public bool setRotation;

        public void Bind(QuantumGame qGame, CutsceneBindingSource bindingSource)
        {
            followTarget = bindingSource.GetMapping(targetTag) as GameObject;

            if (!followTarget) return;
            transform.position = followTarget.transform.position;
            if(setRotation) transform.rotation = followTarget.transform.rotation;
        }

        private void Update()
        {
            if (followTarget) transform.position = followTarget.transform.position;
        }
    }
}