using System;
using Photon.Deterministic;

namespace Quantum
{
    [Serializable]
    public unsafe partial struct ScreenShakeRequest
    {
        public int shakeType;
        public FPVector3 cameraShakeAmount;
        public FPVector3 cameraShakeSpeed;
        public int cameraShakeFrames;
        public int shakeInterval;
    }
}