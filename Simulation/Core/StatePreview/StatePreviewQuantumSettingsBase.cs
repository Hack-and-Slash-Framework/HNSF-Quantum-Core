using Photon.Deterministic;
using UnityEngine.Scripting.APIUpdating;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    [MovedFrom(autoUpdateAPI: true, sourceClassName: "StatePreviewQuantumSettings")]
    public class StatePreviewQuantumSettingsBase : GamemodeSettingsBase
    {
#if QUANTUM_UNITY
        [Header("Standard Settings")]
#endif
        public FP globalDeltaMulti = 1;
        public FP localDeltaMulti = 1;

        public bool activePlayState = false;
        public bool lockStateChange;
    }
}