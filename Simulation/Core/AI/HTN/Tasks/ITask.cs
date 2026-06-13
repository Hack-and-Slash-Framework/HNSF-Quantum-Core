using System.Collections.Generic;
using HnSF.core.AI.HTN.Conditions;
using Quantum;

namespace HnSF.core.AI.HTN.Tasks
{
    public partial interface ITask
    {
        string Label { get; set; }
        byte ID { get; set; }
        ICompoundTask Parent { get; set; }
        List<ICondition> Conditions { get; set; }
        
        ITask ConvertToRuntimeObject(IResourceManager resourceManager);
        void RecursivelyAssignIDs(ITaskIDSource idSource, ref byte id);
        
        bool IsValid(ref HTNAgentContext context);
        DecompositionStatus OnIsValidFailed(ref HTNAgentContext context);
    }
}
