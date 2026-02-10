using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [Serializable]
    public unsafe partial class StateFunctionFPExternal : StateFunctionFP
    {
        public AssetRef<HNSFStateFunctionExternal> externalFunction;
    
        public override FP Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.TryFindAsset(externalFunction, out var ef)) return 0;
            return (ef.function as StateFunctionFP).Execute(frame, entity, ref stateContext);
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new StateFunctionFPExternal());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as StateFunctionFPExternal;
            t.externalFunction = externalFunction;
            return base.CopyTo(target);
        }
    }
}