using System;
using System.Linq;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class CauseScreenShake : HNSFStateAction
    {
        public ScreenShakeRequestParam[] screenShakes = Array.Empty<ScreenShakeRequestParam>();
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            var transform = frame.Unsafe.GetPointer<Transform2D>(entity);

            foreach (var shakeEvent in screenShakes)
            {
                var shakeRequest = shakeEvent.Resolve(frame);
                frame.Events.CauseScreenShake(
                    shakeType: shakeRequest.shakeType,
                    isGlobal: false,
                    origin: transform->Position.XYO,
                    shakeAmount: shakeRequest.cameraShakeAmount,
                    shakeSpeed: shakeRequest.cameraShakeSpeed,
                    shakeFrames: shakeEvent is { type: ScreenShakeRequestParam.ParamType.External, screenShakeFramesOverride: > 0 } ? shakeEvent.screenShakeFramesOverride : shakeRequest.cameraShakeFrames,
                    shakeInterval: shakeRequest.shakeInterval,
                    onlyFor: EntityRef.None);
            }
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new CauseScreenShake());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as CauseScreenShake;
            t.screenShakes = screenShakes.ToArray();
            return base.CopyTo(target);
        }
    }
}