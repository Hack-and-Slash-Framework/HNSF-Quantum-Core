using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial struct ActorInputBufferMovement
    {
        public FPVector2 GetMovement(int offset = 0)
        {
            if (disableReadMovement) return FPVector2.Zero;
            if (offset >= Constants.MOVEMENT_BUFFER_SIZE) offset = Constants.MOVEMENT_BUFFER_SIZE - 1;
            return buffer[(bufferPosition - offset) % (Constants.MOVEMENT_BUFFER_SIZE)];
        }

        public FPVector2 GetFirstMovementInput(short offset, FP validMagnitude, bool returnZeroIfNone = true)
        {
            if (disableReadMovement) return FPVector2.Zero;
            if (offset >= Constants.CAMERA_BUFFER_SIZE) offset = Constants.CAMERA_BUFFER_SIZE - 1;
            for (int i = offset; i < Constants.CAMERA_BUFFER_SIZE; i++)
            {
                var mb = buffer[(bufferPosition - offset) % (Constants.MOVEMENT_BUFFER_SIZE)];
                if (mb.SqrMagnitude >= validMagnitude * validMagnitude) return mb;
            }

            return returnZeroIfNone
                ? FPVector2.Zero
                : buffer[(bufferPosition - Constants.CAMERA_BUFFER_SIZE - 1) % (Constants.MOVEMENT_BUFFER_SIZE)];
        }
    }
}