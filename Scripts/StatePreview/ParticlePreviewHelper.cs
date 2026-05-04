using UnityEngine;

namespace HnSF
{
    public class ParticlePreviewHelper : MonoBehaviour, IEditorAwake
    {
        public ParticleSystem[] particleSystems = new ParticleSystem[0];
        
        public void Awake()
        {
            if (Application.isEditor)
            {
                foreach (ParticleSystem ps in particleSystems)
                {
                    ps.Play(true);
                    ps.Simulate(Time.deltaTime, true, false, false);
                }
                Debug.Log("Preview Helper Particle.");
            }
        }
    }
}