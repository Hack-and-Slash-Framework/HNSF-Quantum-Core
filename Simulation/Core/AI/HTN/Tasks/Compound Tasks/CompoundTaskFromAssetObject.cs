using System;
using System.Collections.Generic;
using HnSF.core.AI.HTN.Conditions;
using Quantum;

namespace HnSF.core.AI.HTN.Tasks
{
    [Serializable]
    public partial class CompoundTaskFromAssetObject : ICompoundTask
    {
        public AssetRef<CompoundTaskAssetObject> assetObjectRef;
        
        public string Label { get; set; }
        public byte ID { get; set; }
        public ICompoundTask Parent { get; set; }
        public List<ICondition> Conditions { get; set; }

        public DecompositionStatus OnIsValidFailed(ref HTNAgentContext context)
        {
            return DecompositionStatus.Failed;
        }
        public bool IsValid(ref HTNAgentContext context)
        {
            return false;
        }
        
        public DecompositionStatus Decompose(ref HTNAgentContext context, byte startIndex, out Queue<byte> result)
        {
            result = null;
            return DecompositionStatus.Failed;
        }
        
        public virtual ITask ConvertToRuntimeObject(IResourceManager resourceManager)
        {
            if (!resourceManager.TryGetAsset(assetObjectRef, out CompoundTaskAssetObject assetObject)) return null;
            return assetObject.ConvertToRuntimeObject(resourceManager);
        }

        public void RecursivelyAssignIDs(ITaskIDSource idSource, ref byte id)
        {
        }
    }
}
