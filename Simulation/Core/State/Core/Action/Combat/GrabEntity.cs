using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class GrabEntity : HNSFStateAction
    {
        public int throweeId;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            if (!frame.Unsafe.TryGetPointer<BoxCombatant>(entity, out var attackerBoxCombatant))
                return false;
            
            EntityRef targetEntityRef = GetActionTargetEntityRef(frame, entity);
            if (targetEntityRef == EntityRef.None) return false;
            
            frame.AddOrGet<IsThrowing>(entity, out var isThrowing);
            var throweesDict = frame.ResolveDictionary(isThrowing->throwees);
            bool didAdd = throweesDict.TryAdd(throweeId, targetEntityRef);

            if (didAdd == false)
            {
                if (throweesDict.Count == 0) frame.Remove<IsThrowing>(entity);
                return false;
            }
            
            var isInThrow = new IsBeingThrown(){ thrower = entity };
            frame.Add(targetEntityRef, isInThrow);

            BoxCombatantHelper.MarkEntityAsTouched(frame, attackerBoxCombatant, targetEntityRef, -1);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new GrabEntity());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as GrabEntity;
            t.throweeId = throweeId;
            return base.CopyTo(target);
        }
    }
}