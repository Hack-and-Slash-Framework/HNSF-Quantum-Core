using System;
using Quantum;

namespace HnSF.core.state.functions
{
    [Serializable]
    public unsafe partial class StateFunctionIntExternal : StateFunctionInt
    {
        public AssetRef<HNSFStateFunctionExternal> externalFunction;
    
        public override int Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.TryFindAsset(externalFunction, out var ef)) return 0;
            return (ef.function as StateFunctionInt).Execute(frame, entity, ref stateContext);
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new StateFunctionIntExternal());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as StateFunctionIntExternal;
            t.externalFunction = externalFunction;
            return base.CopyTo(target);
        }
    }
}