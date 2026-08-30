using HnSF;
using HnSF.core.GroupControl;

namespace Quantum
{
    public unsafe partial struct GroupControlStateData
    {
        public void SetData(AssetRef<BattleActorGroupControlScript> stageScript)
        {
            script = stageScript;
        }

        public BattleScriptResult Initialize(Frame frame, EntityRef entityRef,
            ref BattleScriptContext battleScriptContext)
        {
            frame.TryFindAsset(script, out var ism);
            var lastResult = BattleScriptResult.Succeeded;

            if (currentAction < ism.actions.Count)
            {
                ExecuteOnEnterUntilRunning(frame, entityRef, ref battleScriptContext, ism, ref lastResult);
            }

            return lastResult;
        }

        public BattleScriptResult Tick(Frame frame, EntityRef entityRef, ref BattleScriptContext battleScriptContext)
        {
            frame.TryFindAsset(script, out var bsAsset);

            var lastResult = BattleScriptResult.Succeeded;

            if (currentAction < 0)
                return lastResult;

            while (true)
            {
                if (currentAction < 0 || currentAction >= bsAsset.actions.Count) break;
                lastResult = bsAsset.actions[currentAction].Tick(frame, entityRef, ref battleScriptContext);
                switch (lastResult)
                {
                    case BattleScriptResult.Running:
                        return lastResult;
                    case BattleScriptResult.Succeeded:
                        ExecuteOnExitAndGetNextActionIndex(frame, entityRef, ref battleScriptContext, bsAsset);
                        if (currentAction < 0)
                        {
                            Terminate(frame, entityRef, lastResult, ref battleScriptContext);
                            return lastResult;
                        }

                        break;
                    case BattleScriptResult.Failed:
                    case BattleScriptResult.Canceled:
                        Terminate(frame, entityRef, lastResult, ref battleScriptContext);
                        return lastResult;
                }

                if (currentAction < 0)
                    break;

                if (ExecuteOnEnterUntilRunning(frame, entityRef, ref battleScriptContext, bsAsset, ref lastResult))
                    return lastResult;
            }

            return lastResult;
        }

        private bool ExecuteOnEnterUntilRunning(Frame frame, EntityRef entityRef,
            ref BattleScriptContext battleScriptContext,
            BattleActorGroupControlScript bsAsset, ref BattleScriptResult lastResult)
        {
            bool executeLoop = true;
            while (executeLoop)
            {
                lastResult = bsAsset.actions[currentAction].OnEnter(frame, entityRef, ref battleScriptContext);
                switch (lastResult)
                {
                    case BattleScriptResult.Running:
                        executeLoop = false;
                        break;
                    case BattleScriptResult.Succeeded:
                        ExecuteOnExitAndGetNextActionIndex(frame, entityRef, ref battleScriptContext, bsAsset);
                        if (currentAction < 0)
                        {
                            Terminate(frame, entityRef, lastResult, ref battleScriptContext);
                            return true;
                        }

                        break;
                    case BattleScriptResult.Failed:
                    case BattleScriptResult.Canceled:
                        Terminate(frame, entityRef, lastResult, ref battleScriptContext);
                        return true;
                }

                if (currentAction < 0)
                    break;
            }

            return false;
        }

        private void ExecuteOnExitAndGetNextActionIndex(Frame frame, EntityRef entityRef,
            ref BattleScriptContext battleScriptContext, BattleActorGroupControlScript bsAsset)
        {
            bsAsset.actions[currentAction].OnExit(frame, entityRef, ref battleScriptContext);
            if (bsAsset.actions[currentAction].endExecution)
            {
                currentAction = -1;
                return;
            }

            currentAction = GetNextActionIndex(frame, entityRef, ref battleScriptContext, bsAsset);
        }

        public void Terminate(Frame frame, EntityRef entityRef, BattleScriptResult result,
            ref BattleScriptContext context)
        {
            if (currentAction == -1)
                return;

            currentAction = -1;

            if (!frame.TryFindAsset(script, out var bs))
                return;


            var terminalActions = result switch
            {
                BattleScriptResult.Succeeded => bs.onCompleteActions,
                BattleScriptResult.Failed => bs.onFailActions,
                BattleScriptResult.Canceled => bs.onCancelActions,
                _ => null
            };

            if (terminalActions != null)
            {
                foreach (var action in terminalActions)
                {
                    action.Execute(frame, entityRef, ref context, result);
                }
            }

            if (bs.onTerminateActions != null)
            {
                foreach (var action in bs.onTerminateActions)
                {
                    action.Execute(frame, entityRef, ref context, result);
                }
            }
        }

        private int GetNextActionIndex(Frame frame, EntityRef entityRef, ref BattleScriptContext battleScriptContext,
            BattleActorGroupControlScript bsAsset)
        {
            int nextOperator;
            switch (bsAsset.actions[currentAction].nextExecutedNodeLogic)
            {
                case NextExecutedNodeType.Ordered:
                    for (int i = 0; i < bsAsset.actions[currentAction].nextNodesOrdered.Length; i++)
                    {
                        nextOperator = bsAsset.actions[currentAction].nextNodesOrdered[i];
                        if (nextOperator == -1)
                            continue;

                        if (!bsAsset.actions[nextOperator].IsValid(frame, entityRef, ref battleScriptContext))
                            continue;

                        return nextOperator;
                    }

                    break;
                case NextExecutedNodeType.WeightedRandom:
                    if (bsAsset.actions[currentAction].nextNodesWeighted.TryNext(frame.RNG, out nextOperator))
                    {
                        if (!bsAsset.actions[nextOperator].IsValid(frame, entityRef, ref battleScriptContext))
                            break;
                        return nextOperator;
                    }

                    break;
            }

            return -1;
        }

        public bool IsEnd(Frame frame, ref BattleScriptContext battleScriptContext)
        {
            if (currentAction < 0) return true;
            if (!frame.TryFindAsset(script, out var bsAsset)) return true;
            return currentAction >= bsAsset.actions.Count;
        }
    }
}
