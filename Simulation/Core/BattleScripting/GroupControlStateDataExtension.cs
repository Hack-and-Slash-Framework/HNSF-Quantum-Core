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
        
        public void Initialize(Frame frame, EntityRef entityRef, ref BattleScriptContext battleScriptContext)
        {
            frame.TryFindAsset(script, out var ism);
            var cIndex = currentAction;

            if (cIndex < ism.actions.Count)
            {
                ism.actions[cIndex].OnEnter(frame, entityRef, ref battleScriptContext);
            }
        }

        public bool Tick(Frame frame, EntityRef entityRef, ref BattleScriptContext battleScriptContext)
        {
            frame.TryFindAsset(script, out var bsAsset);
            
            var cIndex = currentAction;
            while (true)
            {
                if (cIndex < 0 || cIndex >= bsAsset.actions.Count) break;
                if (bsAsset.actions[cIndex].Tick(frame, entityRef, ref battleScriptContext))
                {
                    bsAsset.actions[cIndex].OnExit(frame, entityRef, ref battleScriptContext);

                    if (bsAsset.actions[cIndex].endExecution)
                    {
                        currentAction = -1;
                        break;
                    }
                    
                    var nextOperator = -1;
                    switch (bsAsset.actions[cIndex].nextExecutedNodeLogic)
                    {
                        case NextExecutedNodeType.Ordered:
                            var found = false;
                            for (int i = 0; i < bsAsset.actions[cIndex].nextNodesOrdered.Length; i++)
                            {
                                nextOperator = bsAsset.actions[cIndex].nextNodesOrdered[i];
                                if(nextOperator == -1)
                                    continue;
                        
                                if(!bsAsset.actions[nextOperator].IsValid(frame, entityRef, ref battleScriptContext))
                                    continue;
                                
                                currentAction = nextOperator;
                                found = true;
                                break;
                            }
                            if(!found) currentAction = -1;
                            break;
                        case NextExecutedNodeType.WeightedRandom:
                            if (bsAsset.actions[cIndex].nextNodesWeighted.TryNext(frame.RNG, out nextOperator))
                            {
                                if (!bsAsset.actions[nextOperator].IsValid(frame, entityRef, ref battleScriptContext))
                                    break;
                                
                                currentAction = nextOperator;
                            }
                            break;
                    }
                    
                    cIndex = currentAction;

                    if (cIndex >= 0 && cIndex < bsAsset.actions.Count)
                    {
                        bsAsset.actions[cIndex].OnEnter(frame, entityRef, ref battleScriptContext);
                    }
                }
                else
                {
                    break;
                }
                
                if (cIndex == -1 || cIndex >= bsAsset.actions.Count) break;
            }
            return currentAction >= 0 && currentAction < bsAsset.actions.Count;
        }

        public bool IsEnd(Frame frame, ref BattleScriptContext battleScriptContext)
        {
            if (currentAction < 0) return true;
            if(!frame.TryFindAsset(script, out var bsAsset)) return true;
            return currentAction >= bsAsset.actions.Count;
        }
    }
}
