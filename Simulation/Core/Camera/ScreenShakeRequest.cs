using System;
using Photon.Deterministic;

namespace Quantum
{
    [Serializable]
    public unsafe partial struct ScreenShakeRequest
    {
        public int shakeType;
        public FPVector3 cameraShakeStrength;
        public int cameraShakeFrames;
        public int shakeInterval;
    }
}
