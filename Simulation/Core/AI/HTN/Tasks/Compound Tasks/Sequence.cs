using System;
using System.Collections.Generic;
using Quantum;

namespace HnSF.core.AI.HTN.Tasks
{
    /// <summary>
    /// A sequence needs all sub-tasks to decompose successfully to be successful.
    /// </summary>
    [Serializable]
    public unsafe partial class Sequence : CompoundTask
    {
        public override bool IsValid(ref HTNAgentContext context)
        {
            if (base.IsValid(ref context) == false)
                return false;

            if (subtasks.Count == 0)
                return false;

            return true;
        }
        
        protected override DecompositionStatus OnDecompose(ref HTNAgentContext context, byte startIndex,
            out Queue<byte> result)
        {
            result = null;
            return DecompositionStatus.Succeeded;
        }

        protected override DecompositionStatus OnDecomposeTask(ref HTNAgentContext context, ITask task, byte taskIndex,
            int[] oldStackDepth,
            out Queue<byte> result)
        {
            result = null;
            return DecompositionStatus.Succeeded;
        }

        protected override void OnDecomposePrimitiveTask(ref HTNAgentContext context, IPrimitiveTask task,
            byte taskIndex, int[] oldStackDepth,
            out Queue<byte> result)
        {
            result = null;
        }

        protected override DecompositionStatus OnDecomposeCompoundTask(ref HTNAgentContext context, ICompoundTask task,
            byte taskIndex,
            int[] oldStackDepth, out Queue<byte> result)
        {
            result = null;
            return DecompositionStatus.Succeeded;
        }

        public override ITask ConvertToRuntimeObject(IResourceManager resourceManager)
        {
            var copy = new Sequence();
            FillOtherWithValues(copy, resourceManager);
            return copy;
        }
    }
}