using Photon.Deterministic;
using System;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Animation/Set Animation Mixer for Movement")]
    public unsafe partial class SetAnimationMixerForMovement : HNSFStateAction
    {
        public int layer;
        public bool snapping;
        public bool fourWaySnapping;
        public FP snapPercentage;

        public FP smoothing = FP._0;

        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            /*
            var charaInputs = frame.Unsafe.GetPointer<ActorInputInfo>(entity);
            var charaAnimator = frame.Unsafe.GetPointer<BattleActorAnimator>(entity);

            var lookForward = charaInputs->CameraForward;
            var lookRight = charaInputs->CameraRight;
            lookForward.Y = 0;
            lookRight.Y = 0;
            lookForward = lookForward.Normalized;
            lookRight = lookRight.Normalized;

            var mInput = charaInputs->moveInput.Normalized;
            var mVector = InputHelper.GetMovementVector(lookForward, lookRight, mInput);
            mVector.Y = 0;
            mVector = mVector.Normalized;

            var finalMixerParam = mVector.InverseTransformDirection(FPQuaternion.LookRotation(lookForward).Normalized);

            if (fourWaySnapping)
            {
                if (finalMixerParam.X == 0 || finalMixerParam.Z == 0)
                {
                    finalMixerParam = finalMixerParam.Normalized;
                }
                else if (FPMath.Abs(finalMixerParam.X) >= FPMath.Abs(finalMixerParam.Z))
                {
                    finalMixerParam.X = FPMath.Sign(finalMixerParam.X);
                    finalMixerParam.Z = 0;
                }
                else
                {
                    finalMixerParam.X = 0;
                    finalMixerParam.Z = FPMath.Sign(finalMixerParam.Z);
                }
            }
            else if (snapping)
            {
                if (FPMath.Abs(finalMixerParam.X) > snapPercentage)
                {
                    finalMixerParam.X = FPMath.Sign(finalMixerParam.X);
                    finalMixerParam.Z = 0;
                }
                else if (FPMath.Abs(finalMixerParam.Z) > snapPercentage)
                {
                    finalMixerParam.X = 0;
                    finalMixerParam.Z = FPMath.Sign(finalMixerParam.Z);
                }
            }
            
            charaAnimator->state.layers[layer].mixerParam = new FPVector2(finalMixerParam.X, finalMixerParam.Z);
            */
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new SetAnimationMixerForMovement());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as SetAnimationMixerForMovement;
            t.layer = layer;
            t.snapping = snapping;
            t.fourWaySnapping = fourWaySnapping;
            t.snapPercentage = snapPercentage;
            t.smoothing = smoothing;
            return base.CopyTo(target);
        }
    }
}