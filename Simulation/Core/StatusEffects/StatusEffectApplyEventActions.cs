using System;
using System.Collections.Generic;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF.StatusEffects
{
    public unsafe partial class StatusEffectApplyEventActions : StatusEffectAsset
    {
        [Serializable]
        public struct EventActionGrouping
        {
            public EventReceiverTyping eventType;
            public AssetRef<HNSFEventAction> action;
        }
        
#if QUANTUM_UNITY
        [Header("Status Effect")]
#endif
        public List<EventActionGrouping> actionsToApply = new();

        public override void OnApply(Frame frame, EntityRef statusEffectEntityRef, bool asChild = false)
        {
            base.OnApply(frame, statusEffectEntityRef, asChild);

            if (!frame.Unsafe.TryGetPointer<StatusEffector>(statusEffectEntityRef, out var statusEffector)) return;
            
            frame.AddOrGet(statusEffector->actor, out HNSFEventReceiver* eventReceiver);

            foreach (var actionGrouping in actionsToApply)
            {
                eventReceiver->InitializeEventType(frame, (int)actionGrouping.eventType);
                var actionGroups = frame.ResolveDictionary(eventReceiver->actionGroups);

                long tagRef = statusEffectEntityRef.GetHashCode(); // TODO: Prevent overlap
                
                actionGroups[(int)actionGrouping.eventType].Initialize(frame, tagRef);
                actionGroups[(int)actionGrouping.eventType].RegisterAction(frame, tagRef, actionGrouping.action);
            }
        }

        public override void OnRemove(Frame frame, EntityRef statusEffectEntityRef, bool asChild = false)
        {
            base.OnRemove(frame, statusEffectEntityRef, asChild);
            
            if (!frame.Unsafe.TryGetPointer<StatusEffector>(statusEffectEntityRef, out var statusEffector)
                || !frame.Unsafe.TryGetPointer<HNSFEventReceiver>(statusEffector->actor, out var eventReceiver)) return;

            foreach (var actionGrouping in actionsToApply)
            {
                eventReceiver->InitializeEventType(frame, (int)actionGrouping.eventType);
                var actionGroups = frame.ResolveDictionary(eventReceiver->actionGroups);

                long tagRef = statusEffectEntityRef.GetHashCode(); // TODO: Prevent overlap
                
                actionGroups[(int)actionGrouping.eventType].Initialize(frame, tagRef);
                actionGroups[(int)actionGrouping.eventType].UnregisterAction(frame, tagRef, actionGrouping.action);
            }
        }
    }
}