using Quantum;
using UnityEngine;

namespace HnSF
{
    [CreateAssetMenu(menuName = "HnSF/State Preview/Preview Configuration")]
    public class StatePreviewConfiguration : ScriptableObject
    {
        public StatePreviewQuantumConfiguration quantumConfiguration;
        public string previewScene;
    }
}