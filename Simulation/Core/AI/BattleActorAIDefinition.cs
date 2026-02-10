using System;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    [System.Serializable]
    public unsafe partial class BattleActorAIDefinition : AssetObject
    {
#if QUANTUM_UNITY
        [Header("General")]
#endif
        public string label;
        public AssetRef<Tag> mainTag;
        public AssetRef<Tag>[] infoTags = Array.Empty<AssetRef<Tag>>();
        
        public virtual void Setup(Frame frame, EntityRef aiEntityRef, bool debug = false)
        {
            
        }
    }
}