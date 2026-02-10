using System;
using Photon.Deterministic;

namespace Quantum
{
    [Serializable]
    public unsafe partial struct ScreenShakeRequest
    {
        public int shakeType;
        public FP cameraShakeStrength;
        public int cameraShakeFrames;
    }
}
