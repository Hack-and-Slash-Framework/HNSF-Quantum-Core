using System.Collections.Generic;
using Quantum;

namespace HnSF.core.AI.HTN.Tasks
{
    public interface ICompoundTask : ITask
    {
        DecompositionStatus Decompose(ref HTNAgentContext context, byte startIndex, out Queue<byte> result);
    }
}
