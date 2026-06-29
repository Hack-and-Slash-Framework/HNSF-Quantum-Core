using Quantum;

namespace HnSF.core.AI.HTN.Tasks
{
    /// <summary>
    /// A selector that picks a random sub-task to decompose.
    /// </summary>
    public unsafe partial class WeightedRandomSelector : Selector
    {
        public WeightedList<int> actionsWeighted;
        
        public override ITask ConvertToRuntimeObject(IResourceManager resourceManager)
        {
            var copy = new WeightedRandomSelector();
            FillOtherWithValues(copy, resourceManager);
            copy.actionsWeighted = new WeightedList<int>(actionsWeighted);
            return copy;
        }
    }
}