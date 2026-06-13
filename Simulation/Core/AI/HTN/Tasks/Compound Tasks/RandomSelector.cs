using System;
using Quantum;

namespace HnSF.core.AI.HTN.Tasks
{
    /// <summary>
    /// A selector that picks a random sub-task to decompose.
    /// </summary>
    [Serializable]
    public class RandomSelector : Selector
    {
        public override ITask ConvertToRuntimeObject(IResourceManager resourceManager)
        {
            var copy = new RandomSelector();
            FillOtherWithValues(copy, resourceManager);
            return copy;
        }
    }
}