using Quantum;
using UnityEngine;

namespace HnSF
{
    [CreateAssetMenu(menuName = "HnSF/State Preview/Preview Configuration")]
    public class StatePreviewConfiguration : ScriptableObject
    {
        public RuntimeConfig runtimeConfig;
        public StatePreviewQuantumSettingsBase simulationSettings;
        public StatePreviewEntityViewUpdater evuPrefab;
        public SystemsConfigOverrider systemsConfigOverrider;
        public SystemsConfig generatedConfig;
        public string previewScene;
    }
}