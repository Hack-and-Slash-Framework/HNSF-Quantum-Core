using System.Collections.Generic;
using Quantum;

namespace HnSF
{
    public interface ILocalEntityHolder
    {
        Dictionary<int, QuantumEntityView> LocalPlayerEntitys { get; }
    }
}