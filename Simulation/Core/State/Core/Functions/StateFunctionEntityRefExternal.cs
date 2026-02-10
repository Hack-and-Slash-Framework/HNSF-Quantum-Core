using System;
using Quantum;

namespace HnSF.core.state.functions
{
    [Serializable]
    public unsafe partial class StateFunctionEntityRefExternal : StateFunctionEntityRef
    {
        public AssetRef<HNSFStateFunctionExternal> externalFunction;
    
        public override EntityRef Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (!frame.TryFindAsset(externalFunction, out var ef)) return EntityRef.None;
            return (ef.function as StateFunctionEntityRef).Execute(frame, entity, ref stateContext);
        }

        public override HNSFStateFunction Copy()
        {
            return CopyTo(new StateFunctionEntityRefExternal());
        }

        public override HNSFStateFunction CopyTo(HNSFStateFunction target)
        {
            var t = target as StateFunctionEntityRefExternal;
            t.externalFunction = externalFunction;
            return base.CopyTo(target);
        }
    }
}