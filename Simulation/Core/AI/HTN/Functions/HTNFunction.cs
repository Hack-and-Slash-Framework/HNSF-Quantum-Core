using System;
using HnSF.core.GroupControl.Grabbers;
using Photon.Deterministic;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
#endif

namespace HnSF.core.AI.HTN.Functions
{
    [Serializable]
    public unsafe partial class HTNFunction
    {
        public bool disable;
        
        public virtual HTNFunction Copy()
        {
            throw new System.NotImplementedException();
        }

        public virtual HTNFunction CopyTo(HTNFunction target)
        {
            throw new System.NotImplementedException();
        }
    }

    [Serializable]
    public unsafe partial class HTNFunction<T> : HTNFunction
    {
        public virtual T Execute(ref HTNAgentContext context)
        {
            return default(T);
        }
    }

    [Serializable]
    public unsafe partial class HTNFunctionEntityRef : HTNFunction<EntityRef>
    {
        public override EntityRef Execute(ref HTNAgentContext context)
        {
            return default;
        }
    }
    
    [Serializable]
    public unsafe partial class HTNFunctionAssetRef : HTNFunction<AssetRef>
    {
        public override AssetRef Execute(ref HTNAgentContext context)
        {
            return default;
        }
    }

    [Serializable]
    public unsafe partial class HTNFunctionByte : HTNFunction<byte>
    {
        public override byte Execute(ref HTNAgentContext context)
        {
            return 0;
        }
    }
    
    [Serializable]
    public unsafe partial class HTNFunctionInt : HTNFunction<int>
    {
        public override int Execute(ref HTNAgentContext context)
        {
            return 0;
        }
    }
    
    [Serializable]
    public unsafe partial class HTNFunctionFP : HTNFunction<FP>
    {
        public override FP Execute(ref HTNAgentContext context)
        {
            return 0;
        }
    }

    [Serializable]
    public unsafe partial class HTNFunctionFPVector2 : HTNFunction<FPVector2>
    {
        public override FPVector2 Execute(ref HTNAgentContext context)
        {
            return FPVector2.Zero;
        }
    }
    
    [Serializable]
    public unsafe partial class HTNFunctionFPVector3 : HTNFunction<FPVector3>
    {
        public override FPVector3 Execute(ref HTNAgentContext context)
        {
            return FPVector3.Zero;
        }
    }
    
    [Serializable]
    public unsafe partial class HTNFunctionString : HTNFunction<string>
    {
        public override string Execute(ref HTNAgentContext context)
        {
            return String.Empty;
        }
    }
}