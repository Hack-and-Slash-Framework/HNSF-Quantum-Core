using HnSF.core.GroupControl;

namespace Quantum
{
    public unsafe partial struct GroupControlStateData
    {
        public void SetData(AssetRef<BattleActorGroupControlScript> stageScript)
        {
            script = stageScript;
        }
        
        public void Initialize(Frame frame, EntityRef entityRef, ref GroupControlContext groupControlContext)
        {
            frame.TryFindAsset(script, out var ism);
            var cIndex = currentAction;

            if (cIndex < ism.actions.Count)
            {
                ism.actions[cIndex].OnEnter(frame, entityRef, ref groupControlContext);
            }
        }

        public bool Tick(Frame frame, EntityRef entityRef, ref GroupControlContext groupControlContext)
        {
            frame.TryFindAsset(script, out var bsAsset);
            
            var cIndex = currentAction;
            while (true)
            {
                if (cIndex < 0 || cIndex >= bsAsset.actions.Count) break;
                if (bsAsset.actions[cIndex].Tick(frame, entityRef, ref groupControlContext))
                {
                    bsAsset.actions[cIndex].OnExit(frame, entityRef, ref groupControlContext);
                    currentAction++;
                    cIndex = currentAction;

                    if (cIndex < bsAsset.actions.Count)
                    {
                        bsAsset.actions[cIndex].OnEnter(frame, entityRef, ref groupControlContext);
                    }
                }
                else
                {
                    break;
                }
                if (cIndex >= bsAsset.actions.Count) break;
            }
            return currentAction < bsAsset.actions.Count;
        }

        public bool IsEnd(Frame frame, ref GroupControlContext groupControlContext)
        {
            if (currentAction < 0) return true;
            if(!frame.TryFindAsset(script, out var bsAsset)) return true;
            return currentAction >= bsAsset.actions.Count;
        }
    }
}
