using System;
using System.Collections.Generic;
using HnSF.core.AI.HTN.Conditions;
using HnSF.core.AI.HTN.Effects;
using Quantum;
using UnityEngine;

namespace HnSF.core.AI.HTN.Tasks
{
    [Serializable]
    public partial class PrimitiveTaskFromAssetObject : IPrimitiveTask
    {
        public AssetRef<PrimitiveTaskAssetObject> assetObjectRef;
        
        public string Label { get; set; }
        public byte ID { get; set; }
        public int Weight
        {
            get => weight;
            set => weight = value;
        }

        public ICompoundTask Parent { get; set; }
        public List<ICondition> Conditions { get; set; }
        public List<ICondition> ExecutingConditions { get; set; }
        public List<HTNOperatorBase> Operators { get; set; }
        public List<IEffect> Effects { get; set; }

        [SerializeField] protected int weight = 1;
        
        public DecompositionStatus OnIsValidFailed(ref HTNAgentContext context)
        {
            return DecompositionStatus.Failed;
        }
        
        public bool IsValid(ref HTNAgentContext context)
        {
            return false;
        }
        
        public virtual ITask ConvertToRuntimeObject(IResourceManager resourceManager)
        {
            if (!resourceManager.TryGetAsset(assetObjectRef, out PrimitiveTaskAssetObject assetObject)) return null;
            var newTask = assetObject.ConvertToRuntimeObject(resourceManager);
            newTask.Weight = weight;
            return newTask;
        }

        public void RecursivelyAssignIDs(ITaskIDSource idSource, ref byte id)
        {
        }
        
        public void ApplyEffects(ref HTNAgentContext context)
        {
        }

        public void Stop(ref HTNAgentContext context)
        {
        }

        public void Abort(ref HTNAgentContext context)
        {
        }
    }
}