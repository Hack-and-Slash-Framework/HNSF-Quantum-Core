using System;
using System.Collections.Generic;
using HnSF.core.GroupControl.Grabbers;
using Photon.Deterministic;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
#endif

namespace HnSF.core.GroupControl.Functions
{
    public unsafe partial class GroupControlFunction
    {
        [Serializable]
        public unsafe partial class GroupControlFunctionByteList : GroupControlFunction<List<byte>>
        {
            public override List<byte> Execute(Frame frame, EntityRef infoEntityRef)
            {
                return null;
            }
        }
        
        [Serializable]
        public unsafe partial class GroupControlFunctionIntList : GroupControlFunction<List<int>>
        {
            public override List<int> Execute(Frame frame, EntityRef infoEntityRef)
            {
                return null;
            }
        }
        
        [Serializable]
        public unsafe partial class GroupControlFunctionLongList : GroupControlFunction<List<long>>
        {
            public override List<long> Execute(Frame frame, EntityRef infoEntityRef)
            {
                return null;
            }
        }
        
        [Serializable]
        public unsafe partial class GroupControlFunctionFPList : GroupControlFunction<List<FP>>
        {
            public override List<FP> Execute(Frame frame, EntityRef infoEntityRef)
            {
                return null;
            }
        }
        
        [Serializable]
        public unsafe partial class GroupControlFunctionFPVector2List : GroupControlFunction<List<FPVector2>>
        {
            public override List<FPVector2> Execute(Frame frame, EntityRef infoEntityRef)
            {
                return null;
            }
        }
        
        [Serializable]
        public unsafe partial class GroupControlFunctionFPVector3List : GroupControlFunction<List<FPVector3>>
        {
            public override List<FPVector3> Execute(Frame frame, EntityRef infoEntityRef)
            {
                return null;
            }
        }

        [Serializable]
        public unsafe partial class GroupControlFunctionEntityRefList : GroupControlFunction<List<EntityRef>>
        {
            public override List<EntityRef> Execute(Frame frame, EntityRef infoEntityRef)
            {
                return null;
            }
        }
        
        [Serializable]
        public unsafe partial class GroupControlFunctionAssetRefList : GroupControlFunction<List<AssetRef>>
        {
            public override List<AssetRef> Execute(Frame frame, EntityRef infoEntityRef)
            {
                return null;
            }
        }
    }
}
