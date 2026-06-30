using System;
using System.Collections.Generic;
using Quantum;

namespace HnSF.core.AI.HTN.Tasks
{
    /// <summary>
    /// A selector that picks a random sub-task to decompose.
    /// </summary>
    [Serializable]
    public unsafe partial class WeightedRandomSelector : Selector
    {
        public WeightedList<int> actionsWeighted;

        protected override DecompositionStatus OnDecompose(ref HTNAgentContext context, byte startIndex, out Queue<byte> result)
        {
            Plan.Clear();

            var taskIndex = actionsWeighted.Next(context.frame.RNG);
            var task = subtasks[taskIndex];

            return OnDecomposeTask(ref context, task, (byte)taskIndex, null, out result);
        }

        public override ITask ConvertToRuntimeObject(IResourceManager resourceManager)
        {
            var copy = new WeightedRandomSelector();
            FillOtherWithValues(copy, resourceManager);
            copy.actionsWeighted = new WeightedList<int>(actionsWeighted);
            return copy;
        }
    }
}