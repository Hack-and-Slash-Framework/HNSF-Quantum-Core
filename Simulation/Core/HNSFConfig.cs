using System;
using System.Collections.Generic;
using Photon.Deterministic;

namespace Quantum
{
    public partial class HNSFConfig : AssetObject
    {
        public enum EValueType
        {
            None,
            Int,
            Bool,
            Byte,
            FP,
            FPVector2,
            FPVector3,
            String,
            EntityRef,
            AssetRef
        }
    
        [Serializable]
        public class KeyValuePair
        {
            public string Key;
            public EValueType Type;
            public Value Value;
        }
    
        [Serializable]
        public struct Value
        {
            public Int32 Integer;
            public Boolean Boolean;
            public Byte Byte;
            public FP FP;
            public FPVector2 FPVector2;
            public FPVector3 FPVector3;
            public string String;
            public EntityRef EntityRef;
            public AssetRef AssetRef;
        }
    
        public int Count { get { return KeyValuePairs.Count; } }
    
        public List<KeyValuePair> KeyValuePairs = new List<KeyValuePair>(32);
    
    
    }
}