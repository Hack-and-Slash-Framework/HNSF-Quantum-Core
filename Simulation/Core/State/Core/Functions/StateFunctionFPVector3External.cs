using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.functions
{
    [Serializable]
    public unsafe partial class StateFunctionFPVector3External : StateFunctionFPVector3
    {
        public AssetRef<HNSFStateFunctionExternal> externalFunction;
    
        public override FPVector3 Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.TryFindAsset(externalFunction, out var ef)) return FPVector3.Zero;
            return (ef.function as StateFunctionFPVector3).Execute(frame, entity, ref stateContext);
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new StateFunctionFPVector3External());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as StateFunctionFPVector3External;
            t.externalFunction = externalFunction;
            return base.CopyTo(target);
        }
    }
}