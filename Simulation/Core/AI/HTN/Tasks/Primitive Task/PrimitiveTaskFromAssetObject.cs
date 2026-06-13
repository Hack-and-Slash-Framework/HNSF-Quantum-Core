using System;
using System.Collections.Generic;
using HnSF.core.AI.HTN.Conditions;
using HnSF.core.AI.HTN.Effects;
using Quantum;

namespace HnSF.core.AI.HTN.Tasks
{
    [Serializable]
    public partial class PrimitiveTaskFromAssetObject : IPrimitiveTask
    {
        public AssetRef<PrimitiveTaskAssetObject> assetObjectRef;
        
        public string Label { get; set; }
        public byte ID { get; set; }
        public ICompoundTask Parent { get; set; }
        public List<ICondition> Conditions { get; set; }
        public List<ICondition> ExecutingConditions { get; set; }
        public List<HTNOperatorBase> Operators { get; set; }
        public List<IEffect> Effects { get; set; }

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
            return assetObject.ConvertToRuntimeObject(resourceManager);
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