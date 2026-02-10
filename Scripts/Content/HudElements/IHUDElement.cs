using Quantum;
using UnityEngine;

namespace HnSF
{
    public interface IHUDElement
    {
        public Canvas RootCanvas { get; set; }
        public Canvas Canvas { get; }
        public AssetRef<Tag> Tag { get; }
        public void Initialize(QuantumRunner quantumRunner);
        public void Bind(QuantumEntityViewUpdater entityViewUpdater);
        public void Bind(QuantumEntityViewUpdater entityViewUpdater, int participantID);
        public void Teardown();
    }
}