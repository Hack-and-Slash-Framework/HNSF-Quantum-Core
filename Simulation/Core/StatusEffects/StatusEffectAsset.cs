using System;
using HnSF.core.state.decisions;
using HnSF.StatusEffects.Components;
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
        public StatusEffectDuration durationPolicy = StatusEffectDuration.HasDuration;

        [DrawIf(nameof(durationPolicy), (int)StatusEffectDuration.HasDuration)]
        public int durationPerStack = 300;

        public int maxStacks = 1;
        public StackDurationRefreshPolicy stackRefreshPolicy = StackDurationRefreshPolicy.OnSuccessfulApplication;
        public StackExpirationPolicy stackExpirationPolicy = StackExpirationPolicy.RemoveSingleStackAndRefresh;
        public StackingType stackingType = StackingType.PerTarget;
        public bool ignoreOverflowStacks = true;
        public bool shownInHud = true;
        public StatusEffectQualityType qualityType;
        public bool checkStacks = true;

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HNSFStateDecision[] applyConditions = Array.Empty<HNSFStateDecision>();

        public StatusEffectAsset[] childrenStatusEffects = Array.Empty<StatusEffectAsset>();

#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public StatusEffectComponent[] components = Array.Empty<StatusEffectComponent>();

        private void OnValidate()
        {
#if QUANTUM_UNITY
            if (Application.isPlaying)
                return;
            if (components == null)
                return;
            
            foreach(var c in components)
                c.OnValidate(this);
#endif
        }

        /// <summary>
        /// Called when the status effect is applied.
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="statusEffectEntityRef"></param>
        /// <param name="asChild"></param>
        /// <returns>False if the status effect should not be applied. True otherwise.</returns>
        public virtual bool OnApply(Frame frame, EntityRef statusEffectEntityRef, bool asChild = false)
        {
            var statusEffector = frame.Unsafe.GetPointer<StatusEffector>(statusEffectEntityRef);
            
            foreach (var component in components)
            {
                if (component.OnApply(frame, statusEffectEntityRef, statusEffector) == false)
                {
                    return false;
                }
            }

            if (!asChild)
            {
                switch (durationPolicy)
                {
                    case StatusEffectDuration.Instant:
                        break;
                    case StatusEffectDuration.Infinite:
                        break;
                    case StatusEffectDuration.HasDuration:
                        if (durationPerStack <= 0) break;
                        frame.Add<GenericTimer>(statusEffectEntityRef, new GenericTimer()
                        {
                            countingType = TimerCountingType.CountDown,
                            value = durationPerStack
                        });
                        break;
                }
            }

            for (int i = 0; i < this.childrenStatusEffects.Length; i++)
            {
                childrenStatusEffects[i].OnApply(frame, statusEffectEntityRef, true);
            }

            return true;
        }

        public virtual bool OnTick(Frame frame, EntityRef statusEffectEntityRef, bool asChild = false)
        {
            var statusEffector = frame.Unsafe.GetPointer<StatusEffector>(statusEffectEntityRef);
            
            foreach (var component in components)
            {
                if (component.OnTick(frame, statusEffectEntityRef, statusEffector) == false)
                {
                    return false;
                }
            }

            if (!asChild)
            {
                if (checkStacks && frame.Unsafe.TryGetPointer<GenericTimer>(statusEffectEntityRef, out var timer)
                                && timer->value <= 0)
                {
                    statusEffector->stacks--;

                    timer->value = durationPerStack;
                }
            }

            for (int i = 0; i < this.childrenStatusEffects.Length; i++)
            {
                childrenStatusEffects[i].OnTick(frame, statusEffectEntityRef, true);
            }

            return true;
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

        /// <summary>
        /// Called when a status effect is removed.
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="statusEffectEntityRef"></param>
        /// <param name="asChild"></param>
        /// <returns>False if the status effect shouldn't be removed. True otherwise.</returns>
        public virtual bool OnRemove(Frame frame, EntityRef statusEffectEntityRef, bool asChild = false)
        {
            var statusEffector = frame.Unsafe.GetPointer<StatusEffector>(statusEffectEntityRef);
            
            foreach (var component in components)
            {
                if (component.OnRemove(frame, statusEffectEntityRef, statusEffector) == false)
                {
                    return false;
                }
            }

            for (int i = 0; i < this.childrenStatusEffects.Length; i++)
            {
                childrenStatusEffects[i].OnRemove(frame, statusEffectEntityRef, true);
            }

            return true;
        }
    }
}