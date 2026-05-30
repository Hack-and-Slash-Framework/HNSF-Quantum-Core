using Quantum;

namespace HnSF.StatusEffects.Components
{
    [System.Serializable]
    public unsafe partial class TargetHNSFStateTimeRequirement : StatusEffectComponent
    {
        public enum TimeCheckType
        {
            StateFrame,
            TimeSinceStartOfState
        }
        
        public TimeCheckType timeCheckType;
        public int timeRequirement;

        public bool checkOnApply;
        public bool checkOnTick;
        public bool checkOnRemove;
        
        public override bool OnApply(Frame frame, EntityRef statusEffectEntityRef, StatusEffector* statusEffector)
        {
            if (checkOnApply == false)
                return true;
            return Check(frame, statusEffector);
        }

        public override bool OnTick(Frame frame, EntityRef statusEffectEntityRef, StatusEffector* statusEffector)
        {
            if(checkOnTick == false)
                return true;
            return Check(frame, statusEffector);
        }
        
        public override bool OnRemove(Frame frame, EntityRef statusEffectEntityRef, StatusEffector* statusEffector)
        {
            if(checkOnRemove == false)
                return true;
            return Check(frame, statusEffector);
        }
        
        private bool Check(Frame frame, StatusEffector* statusEffector)
        {
            if(!frame.Unsafe.TryGetPointer<HNSFStateAgent>(statusEffector->target, out var hnsfStateAgent))
                return false;

            switch (timeCheckType)
            {
                case TimeCheckType.StateFrame:
                    return hnsfStateAgent->stateData.frame >= timeRequirement;
                case TimeCheckType.TimeSinceStartOfState:
                    return hnsfStateAgent->stateData.realFrame >= timeRequirement;
                default:
                    return false;
            }
        }
    }
}
