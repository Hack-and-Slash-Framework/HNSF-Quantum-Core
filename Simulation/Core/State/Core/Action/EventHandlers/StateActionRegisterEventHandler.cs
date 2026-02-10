using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class StateActionRegisterEventHandler : HNSFStateAction
    {
        public EventRegistryType registryType;
        public EventReceiverTyping eventType;
        public AssetRef<HNSFEventAction>[] actions = Array.Empty<AssetRef<HNSFEventAction>>();

        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent, ref HNSFStateContext stateContext)
        {
            HNSFStateContext targetStateContext = stateContext;
            var targetEntityRef = GetActionTargetEntityRef(frame, entity, ref targetStateContext);
            if (targetEntityRef == EntityRef.None) return false;
            DoAction(frame, targetEntityRef, ref targetStateContext);
            return false;
        }

        protected void DoAction(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            if (actions.Length == 0) return;
            
            frame.AddOrGet<HNSFEventReceiver>(entity, out var eventReceiver);
            eventReceiver->InitializeEventType(frame, (int)eventType);
            var actionGroups = frame.ResolveDictionary(eventReceiver->actionGroups);

            AssetGuid tagRef = default;

            switch (registryType)
            {
                case EventRegistryType.State:
                    tagRef = stateContext.workingState.Id;
                    break;
                case EventRegistryType.Moveset:
                    tagRef = stateContext.agentData->moveset.Id;
                    break;
                case EventRegistryType.Stateset:
                    break;
                case EventRegistryType.Global:
                    tagRef = frame.SimulationConfig.tag_EventHandler_ActorGlobal.Id;
                    break;
            }
            
            actionGroups[(int)eventType].Initialize(frame, tagRef.Value);
            actionGroups[(int)eventType].RegisterActions(frame, tagRef.Value, actions);
            
            if (eventType == EventReceiverTyping.Tick) frame.Add<TickedEventReceiver>(entity);
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new StateActionRegisterEventHandler());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as StateActionRegisterEventHandler;
            t.registryType = registryType;
            t.eventType = eventType;
            t.actions = new AssetRef<HNSFEventAction>[actions.Length];
            Array.Copy(actions, t.actions, actions.Length);
            return base.CopyTo(target);
        }
    }
}