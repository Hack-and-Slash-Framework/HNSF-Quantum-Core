using Quantum;
using UnityEngine;

namespace HnSF
{
    [CreateAssetMenu(menuName = "HnSF/State Preview/Preview Configuration")]
    public class StatePreviewConfiguration : ScriptableObject
    {
        public RuntimeConfig runtimeConfig;
        public StatePreviewQuantumSettings quantumSettings;
        public StatePreviewEntityViewUpdater evuPrefab;
        public string previewScene;
        public EntityPrototype testPrototype;
        public SystemsConfigOverrider systemsConfigOverrider;
        public SystemsConfig generatedConfig;
    }
}