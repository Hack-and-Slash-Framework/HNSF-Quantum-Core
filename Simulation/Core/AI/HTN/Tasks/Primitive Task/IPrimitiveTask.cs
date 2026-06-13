using System.Collections.Generic;
using HnSF.core.AI.HTN.Conditions;
using HnSF.core.AI.HTN.Effects;
using Quantum;

namespace HnSF.core.AI.HTN.Tasks
{
    public interface IPrimitiveTask : ITask
    {
        List<ICondition> ExecutingConditions { get; set; }
        List<HTNOperatorBase> Operators { get; set; }
        List<IEffect> Effects { get; set; }

        void ApplyEffects(ref HTNAgentContext context);

        /// <summary>
        /// Graceful end of task execution.
        /// </summary>
        void Stop(ref HTNAgentContext context);

        /// <summary>
        /// Forced termination of task execution.
        /// </summary>
        void Abort(ref HTNAgentContext context);
    }
}