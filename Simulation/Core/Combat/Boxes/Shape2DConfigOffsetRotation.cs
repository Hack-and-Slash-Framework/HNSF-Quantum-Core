using Photon.Deterministic;

namespace Quantum
{
    public class Shape2DConfigOffsetRotation : AssetObject
    {
        public FPVector2 offset;
        public FP rotation;
        public Shape2DConfig shape = new Shape2DConfig();
    }
}
