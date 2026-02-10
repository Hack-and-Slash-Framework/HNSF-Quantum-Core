using System.Collections;
using System.Collections.Generic;
using Quantum;
using UnityEngine;

namespace HnSF
{
    public interface ILocalEntityHolder
    {
        Dictionary<int, QuantumEntityView> LocalPlayerEntitys { get; }
    }
}