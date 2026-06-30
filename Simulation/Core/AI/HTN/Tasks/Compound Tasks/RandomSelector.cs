using System;
using System.Collections.Generic;
using Quantum;

namespace HnSF.core.AI.HTN.Tasks
{
    /// <summary>
    /// A selector that picks a random sub-task to decompose.
    /// </summary>
    [Serializable]
    public unsafe partial class RandomSelector : Selector
    {
        protected override DecompositionStatus OnDecompose(ref HTNAgentContext context, byte startIndex, out Queue<byte> result)
        {
            Plan.Clear();

            var taskIndex = context.frame.RNG->Next(0, subtasks.Count);
            var task = subtasks[taskIndex];

            return OnDecomposeTask(ref context, task, (byte)taskIndex, null, out result);
        }
        
        public override ITask ConvertToRuntimeObject(IResourceManager resourceManager)
        {
            var copy = new RandomSelector();
            FillOtherWithValues(copy, resourceManager);
            return copy;
        }
    }
}