using Photon.Deterministic;

namespace Quantum
{
    public static unsafe class MathExtensions
    {
        /// <summary>
        /// Transforms a <paramref name="direction" /> from world to local space.
        /// </summary>
        public static FPVector2 InverseTransformDirection(this FPVector2 direction, FP rotation)
        {
            FPMath.SinCosRaw(rotation, out var sinRaw, out var cosRaw);
            FPVector2 fpVector2;
            fpVector2.X.RawValue = (direction.X.RawValue * cosRaw + 32768L /*0x8000*/ >> 16 /*0x10*/) + (direction.Y.RawValue * sinRaw + 32768L /*0x8000*/ >> 16 /*0x10*/);
            fpVector2.Y.RawValue = (direction.Y.RawValue * cosRaw + 32768L /*0x8000*/ >> 16 /*0x10*/) - (direction.X.RawValue * sinRaw + 32768L /*0x8000*/ >> 16 /*0x10*/);
            return fpVector2;
        }
        
        /// <summary>
        /// Transforms a <paramref name="direction" /> from local to world space.
        /// </summary>
        public static FPVector2 TransformDirection(this FPVector2 direction, FP rotation)
        {
            FPMath.SinCosRaw(rotation, out var sinRaw, out var cosRaw);
            FPVector2 fpVector2;
            fpVector2.X.RawValue = (direction.X.RawValue * cosRaw + 32768L /*0x8000*/ >> 16 /*0x10*/) - (direction.Y.RawValue * sinRaw + 32768L /*0x8000*/ >> 16 /*0x10*/);
            fpVector2.Y.RawValue = (direction.X.RawValue * sinRaw + 32768L /*0x8000*/ >> 16 /*0x10*/) + (direction.Y.RawValue * cosRaw + 32768L /*0x8000*/ >> 16 /*0x10*/);
            return fpVector2;
        }
        
        /// <summary>
        /// Transforms a direction from world space to local space. <see cref="F:Quantum.Transform3D.Rotation" /> is expected to be normalized.
        /// </summary>
        public static FPVector3 InverseTransformDirection(this FPVector3 direction, FPQuaternion originRotation)
        {
            return originRotation.Conjugated * direction;
        }
    
        /// <summary>
        /// Transforms a direction from local space to world space. <see cref="F:Quantum.Transform3D.Rotation" /> is expected to be normalized.
        /// </summary>
        public static FPVector3 TransformDirection(this FPVector3 direction, FPQuaternion lookDir) => lookDir * direction;
    }
}