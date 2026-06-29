using System;
using System.Collections.Generic;
using HnSF.core.AI.HTN.Conditions;
using Quantum;
using UnityEngine;

namespace HnSF.core.AI.HTN.Tasks
{
    [Serializable]
    public partial class CompoundTaskFromAssetObject : ICompoundTask
    {
        public AssetRef<CompoundTaskAssetObject> assetObjectRef;
        
        public string Label { get; set; }
        public byte ID { get; set; }
        public int Weight
        {
            get => weight;
            set => weight = value;
        }

        public ICompoundTask Parent { get; set; }
        public List<ICondition> Conditions { get; set; }

        [SerializeField] protected int weight = 1;
        
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
            var newTask = assetObject.ConvertToRuntimeObject(resourceManager);
            newTask.Weight = weight;
            return newTask;
        }

        public void RecursivelyAssignIDs(ITaskIDSource idSource, ref byte id)
        {
        }
    }
}
