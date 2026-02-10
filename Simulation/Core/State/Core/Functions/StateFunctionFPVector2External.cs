using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [Serializable]
    public unsafe partial class StateFunctionFPVector2External : StateFunctionFPVector2
    {
        public AssetRef<HNSFStateFunctionExternal> externalFunction;
    
        public override FPVector2 Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.TryFindAsset(externalFunction, out var ef)) return FPVector2.Zero;
            return (ef.function as StateFunctionFPVector2).Execute(frame, entity, ref stateContext);
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new StateFunctionFPVector2External());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as StateFunctionFPVector2External;
            t.externalFunction = externalFunction;
            return base.CopyTo(target);
        }
    }
}