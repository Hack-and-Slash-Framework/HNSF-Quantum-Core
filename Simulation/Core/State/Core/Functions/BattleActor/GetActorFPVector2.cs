using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class GetActorFPVector2 : StateFunctionFPVector2
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
    
        public override FPVector2 Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            FPVector2 input = FPVector2.Zero;

            for (int i = 0; i < inputSources.Length; i++)
            {
                switch (inputSources[i])
                {
                    case InputSourceType.slope:
                        break;
                    case InputSourceType.stick:
                        break;
                    case InputSourceType.rotation:
                        Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(entity);
                        input = transform->Right;
                        break;
                    case InputSourceType.hardTargetLook:
                        break;
                    case InputSourceType.softTargetLook:
                        break;
                }
                
                if (input != FPVector2.Zero) break;
            }

            return input;
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new GetActorFPVector2());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as GetActorFPVector2;
            t.inputSources = new InputSourceType[inputSources.Length];
            Array.Copy(inputSources, t.inputSources, inputSources.Length);
            return base.CopyTo(target);
        }
    }
}