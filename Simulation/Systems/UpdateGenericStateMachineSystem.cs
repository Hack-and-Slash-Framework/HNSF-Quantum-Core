using HnSF.core.state;
using Quantum;
using Quantum.Profiling;

namespace HnSF.core.systems
{
    public unsafe class UpdateGenericStateMachineSystem : SystemMainThreadFilter<UpdateGenericStateMachineSystem.Filter>, 
        ISignalOnComponentAdded<GenericStateMachine>, ISignalOnComponentRemoved<GenericStateMachine>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public GenericStateMachine* stateAgent;
        }
        
        public override void Update(Frame frame, ref Filter filter)
        {
            if (filter.stateAgent->disableAutoUpdates) return;
            var hasLdt = frame.Unsafe.TryGetPointer<LocalDeltaTime>(filter.Entity, out var ldt);
            if (frame.Has<IsBeingThrown>(filter.Entity)
                || (frame.Unsafe.TryGetPointer<Hitstop>(filter.Entity, out var hitstop) && hitstop->value > 0)
                || (hasLdt && ldt->updatesThisTick == 0))
                return;

            frame.TryFindAsset(filter.stateAgent->config, out AIConfig config);
            
            int updatesThisTick = 1;

            if (hasLdt) updatesThisTick = ldt->updatesThisTick;
            
            HostProfiler.Start("GenericStateMachineSystem::Update");
            for (int i = 0; i < updatesThisTick; i++)
            {
                HNSFStateHelper.Generic.UpdateGenericStateMachine(frame, filter.Entity, filter.stateAgent, config);
            }
            HostProfiler.End();
        }

        public void OnAdded(Frame frame, EntityRef entity, GenericStateMachine* component)
        {
            if (!frame.TryFindAsset<AIBlackboardInitializer>(component->blackboardInitializer.Id, out var initializer)) 
                return;
            AIBlackboardInitializer.InitializeBlackboard(frame, &component->blackboard, initializer);
        }

        public void OnRemoved(Frame frame, EntityRef entity, GenericStateMachine* component)
        {
            component->blackboard.Free(frame);
            component->blackboard = default;
        }
    }
}