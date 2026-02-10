using System;
using HnSF.core.state.decisions;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF.StatusEffects
{
    public unsafe partial class StatusEffectAsset : AssetObject
    {
#if QUANTUM_UNITY
        [Header("General")]
#endif
        public string Label;
        public string displayName;
        public AssetRef<Tag>[] tags = Array.Empty<AssetRef<Tag>>();
        public int maxStacks = 1;
        public int durationPerStack = 300;
        public bool stackCanBeRefreshed = true;
        public bool shownInHud = true;
        public StatusEffectQualityType qualityType;
        public bool checkStacks = true;
        
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HNSFStateDecision[] applyConditions = Array.Empty<HNSFStateDecision>();
        
        public StatusEffectAsset[] childrenStatusEffects = Array.Empty<StatusEffectAsset>();
        
        public virtual void OnApply(Frame frame, EntityRef statusEffectEntityRef, bool asChild = false)
        {
            if (!asChild && durationPerStack > 0)
            {
                frame.Add<GenericTimer>(statusEffectEntityRef, new GenericTimer()
                {
                    countingType = TimerCountingType.CountDown,
                    value = durationPerStack
                });
            }
            
            for (int i = 0; i < this.childrenStatusEffects.Length; i++)
            {
                childrenStatusEffects[i].OnApply(frame, statusEffectEntityRef, true);
            }
        }

        public virtual void OnTick(Frame frame, EntityRef statusEffectEntityRef, bool asChild = false)
        {
            if (!asChild && checkStacks && frame.Unsafe.TryGetPointer<GenericTimer>(statusEffectEntityRef, out var timer)
                && timer->value <= 0)
            {
                if (frame.Unsafe.TryGetPointer<StatusEffector>(statusEffectEntityRef, out var statusEffector))
                {
                    statusEffector->stacks--;
                }
                timer->value = durationPerStack;
            }
            
            for (int i = 0; i < this.childrenStatusEffects.Length; i++)
            {
                childrenStatusEffects[i].OnTick(frame, statusEffectEntityRef, true);
            }
        }
        
        public virtual void OnStackAdded(Frame frame, EntityRef statusEffectEntityRef, int stackDifference,
            bool asChild = false)
        {
            for (int i = 0; i < this.childrenStatusEffects.Length; i++)
            {
                childrenStatusEffects[i].OnStackAdded(frame, statusEffectEntityRef, stackDifference, true);
            }
        }
        
        public virtual void OnStackRemoved(Frame frame, EntityRef statusEffectEntityRef, int stackDifference,
            bool asChild = false)
        {
            for (int i = 0; i < this.childrenStatusEffects.Length; i++)
            {
                childrenStatusEffects[i].OnStackRemoved(frame, statusEffectEntityRef, stackDifference, true);
            }
        }

        public virtual void OnRemove(Frame frame, EntityRef statusEffectEntityRef, bool asChild = false)
        {
            for (int i = 0; i < this.childrenStatusEffects.Length; i++)
            {
                childrenStatusEffects[i].OnRemove(frame, statusEffectEntityRef, true);
            }
        }
    }
}
