using System.Collections.Generic;
using HnSF.core.AI.HTN.Conditions;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF.core.AI.HTN.Tasks
{
    public partial class CompoundTaskAssetObject : AssetObject, ICompoundTask
    {
        public string Label
        {
            get => node.Label; set => node.Label = value;
        }

        public byte ID { get; set; }
        public ICompoundTask Parent { get; set; }
        public List<ICondition> Conditions
        {
            get => node.Conditions;
            set => node.Conditions = value;
        }

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public CompoundTask node;
        
        public bool IsValid(ref HTNAgentContext context)
        {
            return false;
        }

        public DecompositionStatus OnIsValidFailed(ref HTNAgentContext context)
        {
            return DecompositionStatus.Failed;
        }

        public DecompositionStatus Decompose(ref HTNAgentContext context, byte startIndex, out Queue<byte> result)
        {
            result = null;
            return DecompositionStatus.Failed;
        }

        public virtual ITask ConvertToRuntimeObject(IResourceManager resourceManager)
        {
            return node.ConvertToRuntimeObject(resourceManager);
        }

        public void RecursivelyAssignIDs(ITaskIDSource idSource, ref byte id)
        {
        }
    }
}