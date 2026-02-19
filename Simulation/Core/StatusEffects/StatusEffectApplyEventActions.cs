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

        public override bool OnApply(Frame frame, EntityRef statusEffectEntityRef, bool asChild = false)
        {
            if (base.OnApply(frame, statusEffectEntityRef, asChild) == false) return false;

            if (!frame.Unsafe.TryGetPointer<StatusEffector>(statusEffectEntityRef, out var statusEffector)) return true;
            
            frame.AddOrGet(statusEffector->target, out HNSFEventReceiver* eventReceiver);

            foreach (var actionGrouping in actionsToApply)
            {
                eventReceiver->InitializeEventType(frame, (int)actionGrouping.eventType);
                var actionGroups = frame.ResolveDictionary(eventReceiver->actionGroups);

                long tagRef = statusEffectEntityRef.GetHashCode(); // TODO: Prevent overlap
                
                actionGroups[(int)actionGrouping.eventType].Initialize(frame, tagRef);
                actionGroups[(int)actionGrouping.eventType].RegisterAction(frame, tagRef, actionGrouping.action);
            }
            
            return true;
        }

        public override bool OnRemove(Frame frame, EntityRef statusEffectEntityRef, bool asChild = false)
        {
            if (base.OnRemove(frame, statusEffectEntityRef, asChild) == false) return false;

            if (!frame.Unsafe.TryGetPointer<StatusEffector>(statusEffectEntityRef, out var statusEffector)
                || !frame.Unsafe.TryGetPointer<HNSFEventReceiver>(statusEffector->target, out var eventReceiver))
                return true;

            foreach (var actionGrouping in actionsToApply)
            {
                eventReceiver->InitializeEventType(frame, (int)actionGrouping.eventType);
                var actionGroups = frame.ResolveDictionary(eventReceiver->actionGroups);

                long tagRef = statusEffectEntityRef.GetHashCode(); // TODO: Prevent overlap
                
                actionGroups[(int)actionGrouping.eventType].Initialize(frame, tagRef);
                actionGroups[(int)actionGrouping.eventType].UnregisterAction(frame, tagRef, actionGrouping.action);
            }

            return true;
        }
    }
}