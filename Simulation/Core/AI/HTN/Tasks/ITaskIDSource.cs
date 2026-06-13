using System.Collections.Generic;
using HnSF.core.AI.HTN.Tasks;

namespace Quantum
{
    public partial interface ITaskIDSource
    {
        public Dictionary<byte, ITask> IdToTask { get; }
        public Dictionary<ITask, byte> taskToId { get; }
    }
}
