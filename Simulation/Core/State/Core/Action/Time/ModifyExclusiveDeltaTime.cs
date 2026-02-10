using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class ModifyExclusiveDeltaTime : HNSFStateAction
    {
        public enum ModifyType
        {
            Add,
            Remove
        }
        
        public ModifyType modify = ModifyType.Add;
        public int throweeId;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            var targetEntityRef = GetActionTargetEntityRef(frame, entity);
            if (targetEntityRef == EntityRef.None) return false;
            DoAction(frame, targetEntityRef);
            return false;
        }

        private void DoAction(Frame frame, EntityRef entity)
        {
            switch (modify)
            {
                case ModifyType.Add:
                    frame.Add<ExclusiveDeltaTimeActor>(entity);
                    break;
                case ModifyType.Remove:
                    frame.Remove<ExclusiveDeltaTimeActor>(entity);
                    break;
            }
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new ModifyExclusiveDeltaTime());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as ModifyExclusiveDeltaTime;
            t.modify = modify;
            t.throweeId = throweeId;
            return base.CopyTo(target);
        }
    }
}