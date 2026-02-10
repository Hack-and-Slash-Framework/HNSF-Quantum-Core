using System;
using Quantum;

namespace HnSF.core.state.functions
{
    [Serializable]
    public unsafe partial class StateFunctionBoolExternal : StateFunctionBool
    {
        public AssetRef<HNSFStateFunctionExternal> externalFunction;
    
        public override bool Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.TryFindAsset(externalFunction, out var ef)) return false;
            return (ef.function as StateFunctionBool).Execute(frame, entity, ref stateContext);
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new StateFunctionBoolExternal());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as StateFunctionBoolExternal;
            t.externalFunction = externalFunction;
            return base.CopyTo(target);
        }
    }
}