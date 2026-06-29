using System;
using System.Collections.Generic;
using Quantum;
using Quantum.Collections;

namespace HnSF.core.AI.HTN.Tasks
{
    /// <summary>
    /// A selector only needs a single sub-task to decompose for it to be successful.
    /// </summary>
    [Serializable]
    public unsafe partial class Selector : CompoundTask
    {
        protected readonly Queue<byte> Plan = new Queue<byte>();

        public override bool IsValid(ref HTNAgentContext context)
        {
            if (base.IsValid(ref context) == false)
                return false;

            if (subtasks.Count == 0)
                return false;

            return true;
        }

        private bool BeatsLastMTR(ref HTNAgentContext context, int taskIndex, int currentDecompositionIndex)
        {
            var lastMTR = context.frame.ResolveList(context.agent->lastMTR);
            return BeatsLastMTR(ref context, taskIndex, currentDecompositionIndex, ref lastMTR);
        }

        private bool BeatsLastMTR(ref HTNAgentContext context, int taskIndex, int currentDecompositionIndex,
            ref QList<byte> lastMTR)
        {
            // If the last plan's traversal record for this decomposition layer
            // has a smaller index than the current task index we're about to
            // decompose, then the new decomposition can't possibly beat the
            // running plan, so we cancel finding a new plan
            if (lastMTR[currentDecompositionIndex] < taskIndex)
            {
                // But, if any of the earlier records beat the record in LastMTR, we're still good, as we're on a higher priority branch.
                // This ensures that [0,0,1] can beat [0,1,0]
                for (var i = 0; i < context.currentMTR.Count; i++)
                {
                    var diff = context.currentMTR[i] - lastMTR[i];

                    if (diff < 0)
                    {
                        return true;
                    }

                    if (diff > 0)
                    {
                        // We should never really be able to get here, but just in case.
                        return false;
                    }
                }

                return false;
            }

            return true;
        }

        protected override DecompositionStatus OnDecompose(ref HTNAgentContext context, byte startIndex,
            out Queue<byte> result)
        {
            Plan.Clear();
            var lastMTR = context.frame.ResolveList(context.agent->lastMTR);

            if(context.debug) Log.Debug($"Attempting decompose of {Label}, {subtasks.Count}, {startIndex}");
            for (var taskIndex = startIndex; taskIndex < subtasks.Count; taskIndex++)
            {
                if(context.debug) Log.Debug($"{taskIndex}");
                // If the last plan is still running, we need to check whether the
                // new decomposition can possibly beat it.
                if (lastMTR.Count > 0)
                {
                    if(context.debug) Log.Debug("Has lastMTR");
                    if (context.currentMTR.Count < lastMTR.Count)
                    {
                        var currentDecompositionIndex = context.currentMTR.Count;
                        if (BeatsLastMTR(ref context, taskIndex, currentDecompositionIndex, ref lastMTR) == false)
                        {
                            context.currentMTR.Add(0);

                            result = null;
                            Log.Debug("Did not beat lastMTR.");
                            return DecompositionStatus.Rejected;
                        }
                    }
                }

                var task = subtasks[taskIndex];

                // Selector passes null for oldStackDepth: each subtask alternative is independent,
                // so there are no accumulated world state effects to roll back between attempts.
                var status = OnDecomposeTask(ref context, task, taskIndex, null, out result);
                if(context.debug) Log.Debug($"Decompose done. Got {status}. Plan is {result == null}:{result?.Count}");
                switch (status)
                {
                    case DecompositionStatus.Rejected:
                    case DecompositionStatus.Succeeded:
                    case DecompositionStatus.Partial:
                        if(context.debug) Log.Debug("Returning status.");
                        return status;
                    case DecompositionStatus.Failed:
                    default:
                        continue;
                }
            }

            result = Plan;
            return result.Count == 0 ? DecompositionStatus.Failed : DecompositionStatus.Succeeded;
        }

        protected override DecompositionStatus OnDecomposeTask(ref HTNAgentContext context, ITask task, byte taskIndex,
            int[] oldStackDepth,
            out Queue<byte> result)
        {
            if (task.IsValid(ref context) == false)
            {
                if(context.debug) Log.Debug($"Task {taskIndex} was not valid.");
                result = Plan;
                return task.OnIsValidFailed(ref context);
            }

            if (task is ICompoundTask compoundTask)
            {
                return OnDecomposeCompoundTask(ref context, compoundTask, taskIndex, null, out result);
            }

            if (task is IPrimitiveTask primitiveTask)
            {
                OnDecomposePrimitiveTask(ref context, primitiveTask, taskIndex, null, out result);
            }

            /*
            if (task is Slot slot)
            {
                return OnDecomposeSlot(context, slot, taskIndex, null, out result);
            }*/
            
            result = Plan;
            var status = result.Count == 0 ? DecompositionStatus.Failed : DecompositionStatus.Succeeded;
            if(context.debug) Log.Debug($"Task {taskIndex} was valid, status is {status}");
            return status;
        }

        protected override void OnDecomposePrimitiveTask(ref HTNAgentContext context, IPrimitiveTask task,
            byte taskIndex, int[] oldStackDepth,
            out Queue<byte> result)
        {
            context.currentMTR.Add((byte)(taskIndex + 1));

            task.ApplyEffects(ref context);
            Plan.Enqueue(task.ID);
            result = Plan;
        }

        protected override DecompositionStatus OnDecomposeCompoundTask(ref HTNAgentContext context, ICompoundTask task,
            byte taskIndex,
            int[] oldStackDepth, out Queue<byte> result)
        {
            context.currentMTR.Add((byte)(taskIndex + 1));

            var status = task.Decompose(ref context, 0, out var subPlan);

            // If status is rejected, that means the entire planning procedure should cancel.
            if (status == DecompositionStatus.Rejected)
            {
                result = null;
                return DecompositionStatus.Rejected;
            }

            // If the decomposition failed
            if (status == DecompositionStatus.Failed)
            {
                // Remove the taskIndex if it failed to decompose.
                context.currentMTR.RemoveAt(context.currentMTR.Count - 1);

                result = Plan;
                return DecompositionStatus.Failed;
            }
            
            while (subPlan.Count > 0)
            {
                var p = subPlan.Dequeue();
                /*if (context.LogDecomposition)
                {
                    Log(ctx, $"Selector.OnDecomposeCompoundTask:Decomposing {task.Name}:Pushed {p.Name} to plan!", ConsoleColor.Blue);
                }*/
                Plan.Enqueue(p);
            }

            result = Plan;
            var s = result.Count == 0 ? DecompositionStatus.Failed : DecompositionStatus.Succeeded;

            return s;
        }

        /*
        protected override DecompositionStatus OnDecomposeSlot(ref HTNAgentContext context, ICompoundTask task, byte taskIndex,
            int[] oldStackDepth, out Queue<int> result)
        {
        }
        */

        public override ITask ConvertToRuntimeObject(IResourceManager resourceManager)
        {
            var copy = new Selector();
            FillOtherWithValues(copy, resourceManager);
            return copy;
        }
    }
}