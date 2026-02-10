using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class GetActorFPVector3 : StateFunctionFPVector3
    {
        public enum InputSourceType
        {
            none,
            stick,
            rotation,
            slope,
            hardTargetLook,
            softTargetLook,
            custom
        }
        
        public InputSourceType[] inputSources = Array.Empty<InputSourceType>();
    
        public override FPVector3 Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            FPVector3 input = FPVector3.Zero;

            for (int i = 0; i < inputSources.Length; i++)
            {
                switch (inputSources[i])
                {
                    case InputSourceType.slope:
                        break;
                    case InputSourceType.stick:
                        break;
                    case InputSourceType.rotation:
                        Transform3D* transform = frame.Unsafe.GetPointer<Transform3D>(entity);
                        input = transform->Forward;
                        break;
                    case InputSourceType.hardTargetLook:
                        break;
                    case InputSourceType.softTargetLook:
                        break;
                }
                
                if (input != FPVector3.Zero) break;
            }

            return input;
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new GetActorFPVector3());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as GetActorFPVector3;
            t.inputSources = new InputSourceType[inputSources.Length];
            Array.Copy(inputSources, t.inputSources, inputSources.Length);
            return base.CopyTo(target);
        }
    }
}