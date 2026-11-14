using System;
using System.Collections.Generic;
using HnSF.core.state.actions;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF.core.state
{
    [Serializable]
    public class StateActionGrouping
    {
#if QUANTUM_UNITY
        [SerializeReference]
#endif
        public List<HNSFStateAction> data = new List<HNSFStateAction>();
    }
}
