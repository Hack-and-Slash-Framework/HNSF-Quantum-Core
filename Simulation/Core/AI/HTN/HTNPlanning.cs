using System.Collections.Generic;
using HnSF;
using HnSF.core.AI.HTN.Tasks;
using Quantum.Collections;

namespace Quantum
{
    public static unsafe partial class HTNPlanning
    {
        public static void Tick(ref HTNAgentContext context, bool attemptFindPlan = true, bool allowImmediateReplanAndExecute = false)
        {
            var decompositionStatus = DecompositionStatus.Failed;
            var isTryingToReplacePlan = false;
            
            #if UNITY_EDITOR
            if (context.frame.TryFindAsset(context.agent->domainAssetRef, out var domainAsset)
                && domainAsset.remade)
            {
                domainAsset.remade = false;
                ClearPlanForReplan(ref context);
            }
            #endif

            if (attemptFindPlan)
            {
                // Check whether state has changed or the current plan has finished running.
                // and if so, try to find a new plan.
                if (ShouldFindNewPlan(ref context))
                {
                    isTryingToReplacePlan = TryFindNewPlan(ref context, out decompositionStatus);
                    //if(context.debug) Log.Debug($"Should find new plan and got result of {isTryingToReplacePlan}");
                }

                // If the plan has more tasks, we try to select the next one.
                if (CanSelectNextTaskInPlan(ref context))
                {
                    if (context.debug) Log.Debug("Can select next task in plan.");
                    // Select the next task, but check whether the conditions of the next task failed to validate.
                    if (SelectNextTaskInPlan(ref context) == false)
                        return;

                    if (context.agent->CurrentTask(ref context) is IPrimitiveTask taskToStart)
                    {
                        if (TryStartPrimitiveTaskOperator(ref context, taskToStart, allowImmediateReplanAndExecute) ==
                            false)
                            return;
                        if (context.debug) Log.Debug("Started primitive task operator");
                    }
                }
            }

            // If the current task is a primitive task, we try to tick its operator.
            if (context.agent->CurrentTask(ref context) is IPrimitiveTask task)
            {
                if(context.debug) Log.Debug($"Ticking task {task.ID} for {context.agentEntityRef}");
                if (TryTickPrimitiveTaskOperator(ref context, task, allowImmediateReplanAndExecute) == false)
                {
                    return;
                }
            }
            
            // Check whether the planner failed to find a plan
            if (HasFailedToFindPlan(ref context, isTryingToReplacePlan, decompositionStatus))
            {
                if(context.debug) Log.Debug($"$Failed to find plan for {context.agentEntityRef}");
                context.agent->lastStatus = HTNTaskStatus.Failure;
            }
        }
        
        /// <summary>
        /// Check whether state has changed or the current plan has finished running.
        /// and if so, try to find a new plan.
        /// </summary>
        private static bool ShouldFindNewPlan(ref HTNAgentContext context)
        {
            var currentPlan = context.frame.ResolveList(context.agent->currentPlan.tasksToProcess);
            return context.agent->contextDirty || (context.agent->currentPlan.currentTask == 0 && currentPlan.Count == 0);
        }
        
        private static bool TryFindNewPlan(ref HTNAgentContext context, out DecompositionStatus decompositionStatus)
        {
            var currentPlan = context.frame.ResolveList(context.agent->currentPlan.tasksToProcess);
            
            //var lastPartialPlanQueue = PrepareDirtyWorldStateForReplan(ctx);
            var isTryingToReplacePlan = currentPlan.Count > 0;
            
            decompositionStatus = context.agent->FindPlan(ref context, out var newPlan);
            
            if (HasFoundNewPlan(decompositionStatus))
            {
                OnFoundNewPlan(ref context, newPlan);
                if(context.debug) Log.Debug($"Found new plan for {context.agentEntityRef}");
            }
            /*
            else if (lastPartialPlanQueue != null)
            {
                RestoreLastPartialPlan(ctx, lastPartialPlanQueue);
                RestoreLastMethodTraversalRecord(ctx);
            }*/

            return isTryingToReplacePlan;
        }
        
        private static bool HasFoundNewPlan(DecompositionStatus decompositionStatus)
        {
            return decompositionStatus == DecompositionStatus.Succeeded ||
                   decompositionStatus == DecompositionStatus.Partial;
        }
        
        private static void OnFoundNewPlan(ref HTNAgentContext context, Queue<byte> newPlan)
        {
            /*
            if (ctx.PlannerState.OnReplacePlan != null && (ctx.PlannerState.Plan.Count > 0 || ctx.PlannerState.CurrentTask != null))
            {
                ctx.PlannerState.OnReplacePlan.Invoke(ctx.PlannerState.Plan, ctx.PlannerState.CurrentTask, newPlan);
            }
            else if (ctx.PlannerState.OnNewPlan != null && ctx.PlannerState.Plan.Count == 0)
            {
                ctx.PlannerState.OnNewPlan.Invoke(newPlan);
            }*/

            var currentPlan = context.frame.ResolveList(context.agent->currentPlan.tasksToProcess);
            
            currentPlan.Clear();
            while (newPlan.Count > 0)
            {
                currentPlan.Add(newPlan.Dequeue());
            }

            // If a task was running from the previous plan, we stop it.
            if (context.agent->CurrentTask(ref context) is IPrimitiveTask t)
            {
                //ctx.PlannerState.OnStopCurrentTask?.Invoke(t);
                t.Stop(ref context);
                context.agent->currentPlan.currentTask = 0;
            }

            // Copy the MTR into our LastMTR to represent the current plan's decomposition record
            // that must be beat to replace the plan.
            CopyMtrToLastMtr(ref context);
        }
        
        /// <summary>
        /// Copy the MTR into our LastMTR to represent the current plan's decomposition record
        /// that must be beat to replace the plan.
        /// </summary>
        /// <param name="ctx"></param>
        private static void CopyMtrToLastMtr(ref HTNAgentContext context)
        {
            if (context.currentMTR != null)
            {
                var lastMTR = context.frame.ResolveList(context.agent->lastMTR);
                
                lastMTR.Clear();
                foreach (var record in context.currentMTR)
                {
                    lastMTR.Add(record);
                }
                
                /*
                if (ctx.DebugMTR)
                {
                    ctx.LastMTRDebug.Clear();
                    foreach (var record in ctx.MTRDebug)
                    {
                        ctx.LastMTRDebug.Add(record);
                    }
                }*/
            }
        }
        
        /// <summary>
        /// Copy the Last MTR back into our MTR. This is done during rollback when a new plan
        /// failed to beat the last plan.
        /// </summary>
        /// <param name="ctx"></param>
        private static void RestoreLastMethodTraversalRecord(ref HTNAgentContext context)
        {
            var lastMTR = context.frame.ResolveList(context.agent->lastMTR);
            
            if (lastMTR.Count > 0)
            {
                context.currentMTR.Clear();
                foreach (var record in lastMTR)
                {
                    context.currentMTR.Add(record);
                }
                lastMTR.Clear();

                /*
                if (ctx.DebugMTR == false)
                {
                    return;
                }

                ctx.MTRDebug.Clear();
                foreach (var record in ctx.LastMTRDebug)
                {
                    ctx.MTRDebug.Add(record);
                }
                ctx.LastMTRDebug.Clear();*/
            }
        }
        
        /// <summary>
        /// If current task is null, we need to verify that the plan has more tasks queued.
        /// </summary>
        /// <returns></returns>
        private static bool CanSelectNextTaskInPlan(ref HTNAgentContext context)
        {
            var currentPlan = context.frame.ResolveList(context.agent->currentPlan.tasksToProcess);
            return context.agent->currentPlan.currentTask == 0 && currentPlan.Count > 0;
        }
        
        /// <summary>
        /// Dequeues the next task of the plan and checks its conditions. If a condition fails, we require a replan.
        /// </summary>
        private static bool SelectNextTaskInPlan(ref HTNAgentContext context)
        {
            var tasksToProcess = context.frame.ResolveList(context.agent->currentPlan.tasksToProcess);
            //ctx.PlannerState.CurrentTask = ctx.PlannerState.Plan.Dequeue();
            context.agent->currentPlan.currentTask = DequeueNextTaskFromPlan(ref context, tasksToProcess);
            if (context.agent->currentPlan.currentTask != 0)
            {
                //ctx.PlannerState.OnNewTask?.Invoke(ctx.PlannerState.CurrentTask);

                return IsConditionsValid(ref context);
            }

            return true;
        }

        private static byte DequeueNextTaskFromPlan(ref HTNAgentContext context, QList<byte> tasksToProcess)
        {
            if (tasksToProcess.Count == 0)
                return 0;

            var value = tasksToProcess[0];
            tasksToProcess.RemoveAt(0);
            return value;
        }

        /// <summary>
        /// When a new task is selected, we should run Start on its Operator.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="task"></param>
        /// <param name="allowImmediateReplanAndExecute"></param>
        /// <returns></returns>
        private static bool TryStartPrimitiveTaskOperator(ref HTNAgentContext ctx, IPrimitiveTask task, bool allowImmediateReplanAndExecute)
        {
            if (task.Operators != null)
            {
                ctx.agent->currentPlan.currentOperator = 0;
                ctx.agent->lastStatus = task.Operators[ctx.agent->currentPlan.currentOperator].OnEnter(ref ctx);

                if (ctx.agent->lastStatus == HTNTaskStatus.Failure)
                {
                    FailEntirePlan(ref ctx, task, allowImmediateReplanAndExecute);
                    return true;
                }
                
                if (ctx.agent->lastStatus == HTNTaskStatus.Success ||
                    ctx.agent->lastStatus == HTNTaskStatus.Success_DelayOneFrame)
                {
                    task.Operators[ctx.agent->currentPlan.currentOperator].OnExit(ref ctx);
                    
                    SetNextOperator(ref ctx, task);
                    
                    // If all operators completed during start, finish the task now.
                    if (ctx.agent->currentPlan.currentOperator == -1 || ctx.agent->currentPlan.currentOperator >= task.Operators.Count)
                    {
                        // We have to first invoke that the task operator has run its start function successfully, before we report that the operator finished.
                        //ctx.PlannerState.OnCurrentTaskStarted?.Invoke(task);
                        
                        OnOperatorsFinishedSuccessfully(ref ctx, task, allowImmediateReplanAndExecute);
                        return true;
                    }
                    
                    if (ctx.agent->lastStatus == HTNTaskStatus.Success_DelayOneFrame)
                    {
                        return true;
                    }

                    var delayedFrame = false;
                    if (TryEnterNextOperator(ref ctx, task, ref delayedFrame, allowImmediateReplanAndExecute)) return true;
                }

                // Otherwise the operation started as expected, and we are ready to start running Update ticks on the operator.
                //ctx.PlannerState.OnCurrentTaskStarted?.Invoke(task);
                return true;
            }
            
            // This should not really happen if a domain is set up properly.
            task.Abort(ref ctx);
            ctx.agent->currentPlan.currentTask = 0;
            ctx.agent->currentPlan.currentOperator = -1;
            ctx.agent->lastStatus = HTNTaskStatus.Failure;
            if(ctx.debug) Log.DebugError("Should not happen.");
            return true;
        }

        private static void SetNextOperator(ref HTNAgentContext ctx, IPrimitiveTask task)
        {
            var currOperator = ctx.agent->currentPlan.currentOperator;

            if (task.Operators[currOperator].endExecution)
            {
                ctx.agent->currentPlan.currentOperator = -1;
                return;
            }

            int nextOperator;
            switch (task.Operators[currOperator].nextOperatorSelectionType)
            {
                case NextExecutedNodeType.Ordered:
                    for (int i = 0; i < task.Operators[currOperator].nextOperatorsOrdered.Length; i++)
                    {
                        nextOperator = task.Operators[currOperator].nextOperatorsOrdered[i];
                        if(nextOperator == -1)
                            continue;
                        
                        if(!task.Operators[nextOperator].IsValid(ref ctx))
                            continue;

                        ctx.agent->currentPlan.currentOperator = nextOperator;
                        return;
                    }
                    ctx.agent->currentPlan.currentOperator = -1;
                    return;
                case NextExecutedNodeType.WeightedRandom:
                    if (task.Operators[currOperator].nextOperatorsWeighted.TryNext(ctx.frame.RNG, out nextOperator))
                    {
                        if (!task.Operators[nextOperator].IsValid(ref ctx))
                            break;
                        
                        ctx.agent->currentPlan.currentOperator = nextOperator;
                        return;
                    }
                    break;
            }
            ctx.agent->currentPlan.currentOperator = -1;
        }

        /// <summary>
        /// While we have a valid primitive task running, we should tick it each tick of the plan execution.
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="ctx"></param>
        /// <param name="task"></param>
        /// <param name="allowImmediateReplanAndExecute"></param>
        /// <returns></returns>
        private static bool TryTickPrimitiveTaskOperator(ref HTNAgentContext ctx, IPrimitiveTask task, bool allowImmediateReplanAndExecute)
        {
            if (task.Operators != null)
            {
                while (true)
                {
                    if (!IsExecutingConditionsValid(ref ctx, task, allowImmediateReplanAndExecute))
                        return false;
                    
                    ctx.agent->lastStatus = task.Operators[ctx.agent->currentPlan.currentOperator].Tick(ref ctx);

                    var delayedFrame = ctx.agent->lastStatus == HTNTaskStatus.Success_DelayOneFrame;
                    
                    // If the operation failed to finish, we need to fail the entire plan, so that we will replan the next tick.
                    if (ctx.agent->lastStatus == HTNTaskStatus.Failure)
                    {
                        FailEntirePlan(ref ctx, task, allowImmediateReplanAndExecute);
                        return true;
                    }
                    
                    if (ctx.agent->lastStatus == HTNTaskStatus.Success || ctx.agent->lastStatus == HTNTaskStatus.Success_DelayOneFrame)
                    {
                        task.Operators[ctx.agent->currentPlan.currentOperator].OnExit(ref ctx);
                        
                        SetNextOperator(ref ctx, task);
                        
                        // All operators finished successfully, we set task to null so that we dequeue the next task in the plan the following tick.
                        if (ctx.agent->currentPlan.currentOperator == -1 || ctx.agent->currentPlan.currentOperator >= task.Operators.Count)
                        {
                            OnOperatorsFinishedSuccessfully(ref ctx, task, allowImmediateReplanAndExecute);
                            return true;
                        }
                        
                        // Enter next operator.
                        if (TryEnterNextOperator(ref ctx, task, ref delayedFrame)) return true;
                    }
                    else
                    {
                        return true;
                    }

                    if (delayedFrame)
                        return true;

                    // Otherwise the operation isn't done yet and need to continue.
                    //ctx.PlannerState.OnCurrentTaskContinues?.Invoke(task);
                }
                return true;
            }
            
            // This should not really happen if a domain is set up properly.
            task.Abort(ref ctx);
            ctx.agent->currentPlan.currentTask = 0;
            ctx.agent->currentPlan.currentOperator = -1;
            ctx.agent->lastStatus = HTNTaskStatus.Failure;
            if(ctx.debug) Log.DebugError("Should not happen.");
            return true;
        }

        private static bool TryEnterNextOperator(ref HTNAgentContext ctx, IPrimitiveTask task, ref bool delayedFrame, bool allowImmediateReplanAndExecute = false)
        {
            while (true)
            {
                ctx.agent->lastStatus = task.Operators[ctx.agent->currentPlan.currentOperator]
                    .OnEnter(ref ctx);

                if (ctx.agent->lastStatus == HTNTaskStatus.Failure)
                {
                    FailEntirePlan(ref ctx, task, allowImmediateReplanAndExecute);
                    return true;
                }
                            
                if (ctx.agent->lastStatus == HTNTaskStatus.Success ||
                    ctx.agent->lastStatus == HTNTaskStatus.Success_DelayOneFrame)
                {
                    task.Operators[ctx.agent->currentPlan.currentOperator].OnExit(ref ctx);
                    ctx.agent->currentPlan.currentOperator += 1;
                }
                else
                {
                    return true;
                }

                if (ctx.agent->lastStatus == HTNTaskStatus.Success_DelayOneFrame)
                {
                    delayedFrame = true;
                    break;
                }
                            
                if (ctx.agent->currentPlan.currentOperator >= task.Operators.Count)
                {
                    OnOperatorsFinishedSuccessfully(ref ctx, task, allowImmediateReplanAndExecute);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Ensure conditions are valid when a new task is selected from the plan
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        private static bool IsConditionsValid(ref HTNAgentContext ctx)
        {
            foreach (var condition in ctx.agent->CurrentTask(ref ctx).Conditions)
            {
                // If a condition failed, then the plan failed to progress! A replan is required.
                if (condition.IsValid(ref ctx) == false)
                {
                    //ctx.PlannerState.OnNewTaskConditionFailed?.Invoke(ctx.PlannerState.CurrentTask, condition);
                    AbortTask(ref ctx,  ctx.agent->CurrentTask(ref ctx) as IPrimitiveTask);
                    return false;
                }
            }

            return true;
        }
        
        /// <summary>
        /// Ensure executing conditions are valid during plan execution
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="ctx"></param>
        /// <param name="task"></param>
        /// <param name="allowImmediateReplan"></param>
        /// <returns></returns>
        private static bool IsExecutingConditionsValid(ref HTNAgentContext ctx, IPrimitiveTask task, bool allowImmediateReplanAndExecute)
        {
            foreach (var condition in task.ExecutingConditions)
            {
                // If a condition failed, then the plan failed to progress! A replan is required.
                if (condition.IsValid(ref ctx) == false)
                {
                    //ctx.PlannerState.OnCurrentTaskExecutingConditionFailed?.Invoke(task, condition);

                    AbortTask(ref ctx, task);

                    if (allowImmediateReplanAndExecute)
                    {
                        Tick(ref ctx, allowImmediateReplanAndExecute: false);
                    }

                    return false;
                }
            }

            return true;
        }
        
        /// <summary>
        /// When a task is aborted (due to failed condition checks),
        /// we prepare the context for a replan next tick.
        /// </summary>
        /// <param name="ctx"></param>
        private static void AbortTask(ref HTNAgentContext ctx, IPrimitiveTask task)
        {
            task?.Abort(ref ctx);
            ClearPlanForReplan(ref ctx);
        }
        
        /// <summary>
        /// If the operation finished successfully, we set task to null so that we dequeue the next task in the plan the following tick.
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="ctx"></param>
        /// <param name="task"></param>
        /// <param name="allowImmediateReplanAndExecute"></param>
        private static void OnOperatorsFinishedSuccessfully(ref HTNAgentContext ctx, IPrimitiveTask task, bool allowImmediateReplanAndExecute)
        {
            if(ctx.debug) Log.Debug($"OPERATORS FINISHED SUCCESSFULLY: {ctx.agent->currentPlan.currentTask}, {ctx.agent->currentPlan.currentOperator}, {ctx.agent->lastStatus}");
            //ctx.PlannerState.OnCurrentTaskCompletedSuccessfully?.Invoke(task);

            // All effects that is a result of running this task should be applied when the task is a success.
            foreach (var effect in task.Effects)
            {
                if (effect.EffectType == EffectType.PlanAndExecute)
                {
                    //ctx.PlannerState.OnApplyEffect?.Invoke(effect);
                    effect.Apply(ref ctx);
                }
            }

            var currentPlan = ctx.frame.ResolveList(ctx.agent->currentPlan.tasksToProcess);
            ctx.agent->currentPlan.currentTask = 0;
            if (currentPlan.Count == 0)
            {
                var lastMTR = ctx.frame.ResolveList(ctx.agent->lastMTR);
                lastMTR.Clear();

                /*
                if (ctx.DebugMTR)
                {
                    ctx.LastMTRDebug.Clear();
                }*/

                ctx.agent->contextDirty = false;

                if (allowImmediateReplanAndExecute)
                {
                    Tick(ref ctx, allowImmediateReplanAndExecute: false);
                }
            }
        }
        
        /// <summary>
        /// If the operation failed to finish, we need to fail the entire plan, so that we will replan the next tick.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="task"></param>
        private static void FailEntirePlan(ref HTNAgentContext ctx, IPrimitiveTask task, bool allowImmediateReplanAndExecute)
        {
            if(ctx.debug) Log.Debug($"FAILED ENTIRE PLAN: {ctx.agent->lastStatus}, {ctx.agent->currentPlan.currentOperator}, {ctx.agent->currentPlan.currentTask}");
            //ctx.PlannerState.OnCurrentTaskFailed?.Invoke(task);

            task.Abort(ref ctx);
            ClearPlanForReplan(ref ctx);

            if (allowImmediateReplanAndExecute)
            {
                Tick(ref ctx, allowImmediateReplanAndExecute: false);
            }
        }
        
        /// <summary>
        /// Prepare the planner state and context for a clean replan
        /// </summary>
        /// <param name="ctx"></param>
        private static void ClearPlanForReplan(ref HTNAgentContext ctx)
        {
            if(ctx.debug) Log.Debug("CLEAR PLAN FOR REPLAN.");
            ctx.agent->currentPlan.currentTask = 0;
            ctx.agent->currentPlan.currentOperator = 0;
            var currentPlan = ctx.frame.ResolveList(ctx.agent->currentPlan.tasksToProcess);
            currentPlan.Clear();
            
            var lastMTR = ctx.frame.ResolveList(ctx.agent->lastMTR);
            lastMTR.Clear();

            /*
            if (ctx.DebugMTR)
            {
                ctx.LastMTRDebug.Clear();
            }*/

            //ctx.HasPausedPartialPlan = false;
            //ctx.PartialPlanQueue.Clear();
            ctx.agent->contextDirty = false;
        }
        
        /// <summary>
        /// If current task is null, and plan is empty, and we're not trying to replace the current plan, and decomposition failed or was rejected, then the planner failed to find a plan.
        /// </summary>
        /// <param name="isTryingToReplacePlan"></param>
        /// <param name="decompositionStatus"></param>
        /// <returns></returns>
        private static bool HasFailedToFindPlan(ref HTNAgentContext ctx, bool isTryingToReplacePlan, DecompositionStatus decompositionStatus)
        {
            var currentPlan = ctx.frame.ResolveList(ctx.agent->currentPlan.tasksToProcess);
            
            return ctx.agent->currentPlan.currentTask == 0 && currentPlan.Count == 0 && isTryingToReplacePlan == false &&
                   (decompositionStatus == DecompositionStatus.Failed ||
                    decompositionStatus == DecompositionStatus.Rejected);
        }
        
        private static bool HasFailedToFindPlan(ref HTNAgentContext ctx, bool isTryingToReplacePlan, DecompositionStatus decompositionStatus, QList<byte> currentPlan)
        {
            return ctx.agent->currentPlan.currentTask == 0 && currentPlan.Count == 0 && isTryingToReplacePlan == false &&
                   (decompositionStatus == DecompositionStatus.Failed ||
                    decompositionStatus == DecompositionStatus.Rejected);
        }
        
        public static void Reset(ref HTNAgentContext ctx)
        {
            var currentPlan = ctx.frame.ResolveList(ctx.agent->currentPlan.tasksToProcess);
            currentPlan.Clear();

            if (ctx.agent->currentPlan.currentTask != 0 && ctx.agent->CurrentTask(ref ctx) is IPrimitiveTask task)
            {
                task.Stop(ref ctx);
            }

            ClearPlanForReplan(ref ctx);
        }
        
        public static void Reset(ref HTNAgentContext ctx, QList<byte> currentPlan)
        {
            if (ctx.agent->currentPlan.currentTask != 0 && ctx.agent->CurrentTask(ref ctx) is IPrimitiveTask task)
            {
                task.Stop(ref ctx);
            }

            ClearPlanForReplan(ref ctx);
        }
    }
}
