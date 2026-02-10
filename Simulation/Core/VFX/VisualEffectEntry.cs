#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    public partial class VisualEffectEntry : AssetObject
    {
#if QUANTUM_UNITY
        public GameObject visualEffect;
#endif
    }
}