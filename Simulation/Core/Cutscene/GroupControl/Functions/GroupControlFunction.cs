using System;
using HnSF.core.GroupControl.Grabbers;
using Photon.Deterministic;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
#endif

namespace HnSF.core.GroupControl.Functions
{
    [Serializable]
    public unsafe partial class GroupControlFunction
    {
        public string Label;
        public bool disable;
        
        public virtual GroupControlFunction Copy()
        {
            throw new System.NotImplementedException();
        }

        public virtual GroupControlFunction CopyTo(GroupControlFunction target)
        {
            throw new System.NotImplementedException();
        }
    }

    [Serializable]
    public unsafe partial class GroupControlFunction<T> : GroupControlFunction
    {
        public virtual T Execute(Frame frame, EntityRef infoEntityRef)
        {
            return default(T);
        }
    }

    [Serializable]
    public unsafe partial class GroupControlFunctionEntityRef : GroupControlFunction<EntityRef>
    {
        public override EntityRef Execute(Frame frame, EntityRef infoEntityRef)
        {
            return default;
        }
    }
    
    [Serializable]
    public unsafe partial class GroupControlFunctionAssetRef : GroupControlFunction<AssetRef>
    {
        public override AssetRef Execute(Frame frame, EntityRef infoEntityRef)
        {
            return default;
        }
    }

    [Serializable]
    public unsafe partial class GroupControlFunctionInt : GroupControlFunction<int>
    {
        public override int Execute(Frame frame, EntityRef infoEntityRef)
        {
            return 0;
        }
    }
    
    [Serializable]
    public unsafe partial class GroupControlFunctionFP : GroupControlFunction<FP>
    {
        public override FP Execute(Frame frame, EntityRef infoEntityRef)
        {
            return 0;
        }
    }

    [Serializable]
    public unsafe partial class GroupControlFunctionFPVector2 : GroupControlFunction<FPVector2>
    {
        public override FPVector2 Execute(Frame frame, EntityRef infoEntityRef)
        {
            return FPVector2.Zero;
        }
    }
    
    [Serializable]
    public unsafe partial class GroupControlFunctionFPVector3 : GroupControlFunction<FPVector3>
    {
        public override FPVector3 Execute(Frame frame, EntityRef infoEntityRef)
        {
            return FPVector3.Zero;
        }
    }
}