#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    public partial class Tag : AssetObject
    {
        public string label;
#if QUANTUM_UNITY
        [TextArea]
#endif
        public string description;
    }
}