using Photon.Deterministic;

namespace Quantum
{
    public class Shape3DConfigOffsetRotation : AssetObject
    {
        public FPVector3 offset;
        public FPVector3 rotation;
        public Shape3DConfig shape = new Shape3DConfig();
    }
}