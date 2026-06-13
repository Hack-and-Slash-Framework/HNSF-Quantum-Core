using System.Collections.Generic;
using HnSF.core.AI.HTN.Tasks;

namespace Quantum
{
    public unsafe partial struct HTNAgent
    {
        public void Clear()
        {
            domainAssetRef = default;
            currentPlan.currentTask = 0;
            currentPlan.currentOperator = -1;
            uninterruptible = false;
        }

        public void ResetCooldown()
        {
            cooldown = 0;
        }

        public ITask CurrentTask(ref HTNAgentContext context)
        {
            if (context.agent->currentPlan.currentTask == 0 || !context.frame.TryFindAsset(context.agent->domainAssetRef, out var domainAsset))
                return null;
            return domainAsset.IdToTask.GetValueOrDefault(context.agent->currentPlan.currentTask);
        }

        public DecompositionStatus FindPlan(ref HTNAgentContext context, out Queue<byte> plan)
        {
            Log.Debug("Attempting to find plan.");
            context.agent->contextState = HTNContextState.Planning;
            
            plan = null;
            var status = DecompositionStatus.Rejected;

            status = OnReplanDuringPartialPlanning(ref context, ref plan, status);
            
            if (HasFoundSamePlan(ref context))
            {
                plan = null;
                status = DecompositionStatus.Rejected;
            }

            if (HasDecompositionSucceeded(status))
            {
                ApplyPermanentWorldStateStackChanges(ref context);
            }
            else
            {
                ClearWorldStateStackChanges(ref context);
            }

            context.agent->contextState = HTNContextState.Executing;
            return status;
        }
        
        /// <summary>
        /// We first check whether we have a stored start task. This is true
        /// if we had a partial plan pause somewhere in our plan, and we now
        /// want to continue where we left off.
        /// If this is the case, we don't erase the MTR, but continue building it.
        /// However, if we have a partial plan, but LastMTR is not 0, that means
        /// that the partial plan is still running, but something triggered a replan.
        /// When this happens, we have to plan from the domain root (we're not
        /// continuing the current plan), so that we're open for other plans to replace
        /// the running partial plan.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="plan"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private DecompositionStatus OnReplanDuringPartialPlanning(ref HTNAgentContext context, ref Queue<byte> plan, DecompositionStatus status)
        {
            //var lastPartialPlanQueue = CacheLastPartialPlan(ctx);

            ClearMethodTraversalRecord(ref context);

            if (!context.frame.TryFindAsset(domainAssetRef, out var domainAsset))
            {
                if(context.debug) Log.Debug("Could not find domain asset. Failing replan.");
                return DecompositionStatus.Failed;
            }
            if(context.debug) Log.Debug("Attempting decompose.");
            
            // Replan through decomposition of the hierarchy
            status = domainAsset.runtimeRoot.Decompose(ref context, 0, out plan);

            if (HasDecompositionFailed(status))
            {
                if(context.debug) Log.Debug("Decomposition failed. Failing replan.");
                //RestoreLastPartialPlan(ctx, lastPartialPlanQueue, status);
            }
            return status;
        }
        
        /*
        /// <summary>
        /// If we failed to find a new plan, we have to restore the old plan,
        /// if it was a partial plan.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="lastPartialPlanQueue"></param>
        /// <param name="status"></param>
        private void RestoreLastPartialPlan(ref HTNAgentContext context, Queue<PartialPlanEntry> lastPartialPlanQueue, DecompositionStatus status)
        {
            if (lastPartialPlanQueue == null)
            {
                return;
            }

            ctx.HasPausedPartialPlan = true;
            ctx.PartialPlanQueue.Clear();

            while (lastPartialPlanQueue.Count > 0)
            {
                ctx.PartialPlanQueue.Enqueue(lastPartialPlanQueue.Dequeue());
            }

            ctx.Factory.FreeQueue(ref lastPartialPlanQueue);
        }*/
        
        public void ClearMethodTraversalRecord(Frame frame)
        {
            var mtrList = frame.ResolveList(lastMTR);
            mtrList.Clear();
        }
        
        private void ClearMethodTraversalRecord(ref HTNAgentContext context)
        {
            var mtrList = context.frame.ResolveList(context.agent->lastMTR);
            mtrList.Clear();
        }

        private bool HasDecompositionFailed(DecompositionStatus status)
        {
            return status == DecompositionStatus.Rejected || status == DecompositionStatus.Failed;
        }

        private bool HasDecompositionSucceeded(DecompositionStatus status)
        {
            return status == DecompositionStatus.Succeeded || status == DecompositionStatus.Partial;
        }

        private DecompositionStatus OnPausedPartialPlan(ref HTNAgentContext context, ref Queue<ITask> plan, DecompositionStatus status)
        {
            return DecompositionStatus.Failed;
        }

        /// <summary>
        /// Enqueues the sub plan's queue onto the existing plan
        /// </summary>
        /// <param name="plan"></param>
        /// <param name="subPlan"></param>
        private void EnqueueToExistingPlan(ref Queue<ITask> plan, Queue<ITask> subPlan)
        {
            while (subPlan.Count > 0)
            {
                plan.Enqueue(subPlan.Dequeue());
            }
        }
        
        /// <summary>
        /// If this MTR equals the last MTR, then we need to double-check whether we ended up
        /// just finding the exact same plan. During decomposition each compound task can't check
        /// for equality, only for less than, so this case needs to be treated after the fact.
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        private bool HasFoundSamePlan(ref HTNAgentContext context)
        {
            var lastMtrList = context.frame.ResolveList(context.agent->lastMTR);
            
            var isMTRsEqual = context.currentMTR.Count == lastMtrList.Count;
            if (isMTRsEqual)
            {
                for (var i = 0; i < context.currentMTR.Count; i++)
                {
                    if (context.currentMTR[i] < lastMtrList[i])
                    {
                        isMTRsEqual = false;
                        break;
                    }
                }

                return isMTRsEqual;
            }

            return false;
        }
        
        /// <summary>
        /// Apply permanent world state changes to the actual world state used during plan execution.
        /// </summary>
        /// <param name="ctx"></param>
        private void ApplyPermanentWorldStateStackChanges(ref HTNAgentContext context)
        {
            var currentWorldState = context.frame.ResolveDictionary(context.agent->worldState.current);
            
            // Trim away any plan-only or plan&execute effects from the world state change stack, that only
            // permanent effects on the world state remains now that the planning is done.
            context.TrimForExecution();

            foreach (var stateChangeStack in context.worldStateChangeStack)
            {
                var stack = stateChangeStack.Value;
                if (stack != null && stack.Count > 0)
                {
                    var sItem = stack.Peek();
                    currentWorldState[stateChangeStack.Key] = sItem.Value;
                    stack.Clear();
                }
            }
        }
        
        /// <summary>
        /// Clear away any changes that might have been applied to the stack
        /// </summary>
        /// <param name="ctx"></param>
        private void ClearWorldStateStackChanges(ref HTNAgentContext context)
        {
            foreach (var stack in context.worldStateChangeStack)
            {
                if (stack.Value != null && stack.Value.Count > 0)
                {
                    stack.Value.Clear();
                }
            }
        }
    }
}
