using Quantum;

namespace HnSF.core.systems
{
    public unsafe class ActorAIUpdate : SystemMainThreadFilter<ActorAIUpdate.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public BattleActorAI* battleActorAI;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            if (!f.IsVerified) return;

            if (filter.battleActorAI->updateInterval > 0)
            {
                var frameSlice = f.Number % filter.battleActorAI->updateInterval;
                var entitySlice = filter.Entity.Index % filter.battleActorAI->updateInterval;
                if (frameSlice != entitySlice) return;
            }

            var aiContext = new AIContext();

            if (f.Unsafe.TryGetPointer<HFSMCompoundAgent>(filter.Entity, out var compoundAgent))
            {
                var aiContextUser = new AIContextUser(&compoundAgent->BrainBb, &compoundAgent->Brain, AIContextUserType.BattleActorHFSMCompound, null);
                aiContext.SetHFSMAgentAndBlackboard(&compoundAgent->Brain, filter.Entity, &compoundAgent->BrainBb);
                aiContext.SetUserData(&aiContextUser);
                HFSMManager.Update(f, f.DeltaTime, filter.Entity, ref aiContext);
            }
        }
    }
}