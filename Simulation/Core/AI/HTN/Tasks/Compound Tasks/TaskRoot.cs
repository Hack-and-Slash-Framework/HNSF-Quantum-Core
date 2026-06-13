using System;
using Quantum;

namespace HnSF.core.AI.HTN.Tasks
{
    [Serializable]
    public unsafe partial class TaskRoot : Selector
    {
        public override ITask ConvertToRuntimeObject(IResourceManager resourceManager)
        {
            var copy = new TaskRoot();
            FillOtherWithValues(copy, resourceManager);
            return copy;
        }
    }
}
