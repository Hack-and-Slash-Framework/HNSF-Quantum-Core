#if HNSF_ANIMANCER
using Animancer;
using System;
using Quantum;
using System.Collections.Generic;
using UnityEngine;

namespace HnSF
{
    public unsafe class EntityAnimationUpdaterAnimancer : EntityAnimationUpdaterBase
    {
        [System.Serializable]
        public class TagToAnimator
        {
            public AssetRef<Tag> tag;
            public AnimancerComponent animancer;
        }

        [System.Serializable]
        public class AnimatorInfo
        {
            public Dictionary<AssetRef<AnimationEntry>, MixerState<Vector2>> mixers = new();
            public Dictionary<AssetRef<AnimationEntry>, MixerState<float>> lMixers = new();

            public AnimancerComponent animancer;

            public Dictionary<int, AnimancerLayer> layers = new();
            public Dictionary<int, AnimancerState> states = new();

            public Dictionary<int, AnimationEntry.MixerType> layerMixerType =
                new Dictionary<int, AnimationEntry.MixerType>();
        }

        [Header("Animancer")] public TagToAnimator[] animators;
        [NonSerialized] public Dictionary<AssetRef<Tag>, AnimatorInfo> animatorInfoGroups = new();

        public override void Awake()
        {
            animatorInfoGroups.Clear();
            foreach (var a in animators)
            {
                var aInfo = new AnimatorInfo()
                {
                    animancer = a.animancer,
                    mixers = new(),
                    lMixers = new(),
                    layers = new Dictionary<int, AnimancerLayer>(),
                    layerMixerType = new Dictionary<int, AnimationEntry.MixerType>(),
                    states = new Dictionary<int, AnimancerState>()
                };

                aInfo.layers.Add(0, a.animancer.Layers[0]);
                aInfo.layers.Add(1, a.animancer.Layers[1]);
                aInfo.layerMixerType.Add(0, AnimationEntry.MixerType.none);
                aInfo.layerMixerType.Add(1, AnimationEntry.MixerType.none);
                aInfo.states.Add(0, null);
                aInfo.states.Add(1, null);

                animatorInfoGroups.Add(a.tag, aInfo);
            }

            foreach (var a in animators) a.animancer.Graph.PauseGraph();
            base.Awake();
        }

        protected virtual bool TrySetupAnimation(AssetRef<AnimationEntry> animationEntry)
        {
            return TrySetupAnimation(QuantumUnityDB.GetGlobalAsset(animationEntry));
        }

        protected virtual bool TrySetupAnimation(AnimationEntry grouping)
        {
            if (grouping == null || grouping.mixer == AnimationEntry.MixerType.none || grouping.animsTargets == null ||
                grouping.animsTargets.Length == 0) return true;

            foreach (var at in grouping.animsTargets)
            {
                if (animatorInfoGroups.TryGetValue(at.animTargetTag, out var animInfoGroup) == false) continue;

                switch (grouping.mixer)
                {
                    case AnimationEntry.MixerType.Linear:
                        if (animInfoGroup.lMixers.ContainsKey(grouping)) break;
                        var lMix = new LinearMixerState();
                        foreach (var entry in at.anims)
                        {
                            lMix.Add(entry.clip, entry.param.x);
                        }

                        animInfoGroup.lMixers.Add(grouping, lMix);
                        break;
                    case AnimationEntry.MixerType.Cartesian:
                        if (animInfoGroup.mixers.ContainsKey(grouping)) break;
                        var mix = new CartesianMixerState();
                        foreach (var entry in at.anims)
                        {
                            mix.Add(entry.clip, entry.param);
                        }

                        animInfoGroup.mixers.Add(grouping, mix);
                        break;
                    case AnimationEntry.MixerType.Directional:
                        if (animInfoGroup.mixers.ContainsKey(grouping)) break;
                        var dmix = new DirectionalMixerState();
                        foreach (var entry in at.anims)
                        {
                            dmix.Add(entry.clip, entry.param);
                        }

                        animInfoGroup.mixers.Add(grouping, dmix);
                        break;
                }
            }

            return true;
        }

        public override void UpdateAnimatorState(QuantumGame game)
        {
            if (disabled || !renderersList.IsVisible() || !game.Frames.Predicted.Exists(entityView.EntityRef)) return;
            PauseAnimators();

            var ldt = game.Frames.Predicted.Unsafe.GetPointer<LocalDeltaTime>(entityView.EntityRef);
            var effectiveFixedDT = game.Frames.Predicted.DeltaTime.AsFloat / ldt->multiplier.AsFloat;

            if (lastFrameUpdateNumber != game.Frames.Predicted.Number &&
                game.Frames.Predicted.TryGet<BattleActorAnimator>(entityView.EntityRef,
                    out var cAna) && ldt->updatesThisTick > 0)
            {
                lastFrameUpdateNumber = game.Frames.Predicted.Number;
                accumulatedTimeSinceLastUpdate = 0;
                actorAnimatorLast = actorAnimator;
                actorAnimator = cAna;

                // Update animations if they changed.
                for (int i = 0; i < actorAnimator.state.layers.Length; i++)
                {
                    if (actorAnimator.state.layers[i].animationEntry ==
                        actorAnimatorLast.state.layers[i].animationEntry) continue;
                    TrySetupAnimation(actorAnimator.state.layers[i].animationEntry);
                    PlayAnimationForLayer(i, actorAnimatorLast.state.layers[i].animationEntry, actorAnimator.state.layers[i].animationEntry,
                        actorAnimator.state.layers[i].mask);
                }
            }


            if ((game.Frames.Predicted.Unsafe.TryGetPointer<Hitstop>(entityView.EntityRef, out var selfHitstop) &&
                 selfHitstop->value > 0))
            {
                SetAnimationLayerPlayState(0, false);
                SetAnimationLayerPlayState(1, false);
                EvaluateAnimators(effectiveFixedDT);
#if UNITY_EDITOR
                if(Application.isEditor) accumulatedTimeSinceLastUpdate += (float)EditorDeltaTime.editorDeltaTime;
                else accumulatedTimeSinceLastUpdate += Time.deltaTime;
#else
                accumulatedTimeSinceLastUpdate += Time.deltaTime;
#endif
                return;
            }

            SetAnimationLayerPlayState(0, true);
            SetAnimationLayerPlayState(1, true);

            var alpha = accumulatedTimeSinceLastUpdate / (effectiveFixedDT); //game.InterpolationFactor;
            var fdt = game.Frames.Predicted.DeltaTime.AsFloat;

            // Interpolation.
            HandleInterpolation(0, fdt, alpha);
            HandleInterpolation(1, fdt, alpha);

            _lastSetState = actorAnimator.state;
            EvaluateAnimators(effectiveFixedDT);

#if UNITY_EDITOR
            if(Application.isEditor) accumulatedTimeSinceLastUpdate += (float)EditorDeltaTime.editorDeltaTime;
            else accumulatedTimeSinceLastUpdate += Time.deltaTime;
#else
            accumulatedTimeSinceLastUpdate += Time.deltaTime;
#endif
        }

        protected virtual void HandleInterpolation(int layer, float fdt, float alpha)
        {
            float lastFrameWeight = actorAnimator.state.layers[layer].weight.AsFloat;
            int lastFrame = actorAnimator.state.layers[layer].frame;
            Vector2 lastFrameLayerParam = actorAnimator.state.layers[layer].mixerParam.ToUnityVector2();

            float currentFrameWeight = lastFrameWeight;
            int currentFrame = lastFrame;
            Vector2 currentFrameLayerParam = lastFrameLayerParam;

            if (actorAnimatorLast.state.layers[layer].animationEntry ==
                actorAnimator.state.layers[layer].animationEntry)
            {
                lastFrameWeight = actorAnimatorLast.state.layers[layer].weight.AsFloat;
                lastFrame = actorAnimatorLast.state.layers[layer].frame;
                lastFrame = Mathf.Clamp(lastFrame, 0, currentFrame);
                lastFrameLayerParam = actorAnimatorLast.state.layers[layer].mixerParam.ToUnityVector2();
            }

            float lastFrameTime = lastFrame * fdt;
            float currentFrameTime = currentFrame * fdt;

            float finalWeight = Mathf.Lerp(lastFrameWeight, currentFrameWeight, alpha);
            Vector2 blendedLayerParam = Vector2.Lerp(lastFrameLayerParam, currentFrameLayerParam, alpha);

            // Update Layer time.
            SetAnimationLayerTime(layer, Mathf.Lerp(lastFrameTime, currentFrameTime, alpha));

            // Update Layer weight.
            SetLayerWeight(layer, finalWeight);

            // Update layer param.
            UpdateMixers(layer, blendedLayerParam);
        }

        public AnimancerComponent GetDefaultAnimancer()
        {
            return animators[0].animancer;
        }

        protected virtual void SetAnimationLayerPlayState(int layer, bool playState)
        {
            foreach (var group in animatorInfoGroups)
            {
                if (group.Value.states[layer] == null) continue;
                group.Value.states[layer].IsPlaying = playState;
            }
        }

        protected virtual void SetAnimationLayerTime(int layer, float time)
        {
            foreach (var group in animatorInfoGroups)
            {
                if (group.Value.states[layer] == null) continue;
                group.Value.states[layer].Time = time * group.Value.states[layer].Speed;
            }
        }

        protected virtual void PlayAnimationForLayer(int layer, AssetRef<AnimationEntry> lastEntry, AssetRef<AnimationEntry> entry,
            AssetRef<Tag> avatarMaskTag)
        {
            var animEntry = QuantumUnityDB.GetGlobalAsset<AnimationEntry>(entry.Id);

            tagToAvatarMaskMapping.TryGetValue(avatarMaskTag, out var avatarMask);

            float wantedFadeDuration = GetFadeTimeFor(lastEntry, entry);
            foreach (var group in animatorInfoGroups)
            {
                if (animEntry == null || !animEntry.HasTarget(group.Key))
                {
                    if (group.Value.layers[layer].Mask != null) group.Value.layers[layer].Mask = null;
                    if (layer != 0) group.Value.layers[layer].Stop();
                    //if (layer != 0) group.Value.layers[layer].Weight = 0;
                    //if(layer == 0) group.Value.animancer.gameObject.SetActive(false);
                    continue;
                }

                var anims = animEntry.GetAnimsForTarget(group.Key);
                if (anims == null || anims.Length == 0)
                {
                    if (group.Value.layers[layer].Mask != null) group.Value.layers[layer].Mask = null;
                    if (layer != 0) group.Value.layers[layer].Stop();
                    //if (layer != 0) group.Value.layers[layer].Weight = 0;
                    //if(layer == 0) group.Value.animancer.gameObject.SetActive(false);
                    continue;
                }

                if (!group.Value.animancer.gameObject.activeInHierarchy)
                    group.Value.animancer.gameObject.SetActive(true);
                
                group.Value.layerMixerType[layer] = animEntry.mixer;

                switch (animEntry.mixer)
                {
                    case AnimationEntry.MixerType.none:
                        var clip = anims[0].clip;
                        var blp = group.Value.layers[layer].Play(clip, wantedFadeDuration);
                        blp.Speed = animEntry.playRate;
                        group.Value.states[layer] = blp;
                        break;
                    case AnimationEntry.MixerType.Cartesian:
                    case AnimationEntry.MixerType.Directional:
                        group.Value.states[layer] =
                            group.Value.layers[layer].Play(group.Value.mixers[animEntry], wantedFadeDuration);
                        break;
                    case AnimationEntry.MixerType.Linear:
                        group.Value.states[layer] =
                            group.Value.layers[layer].Play(group.Value.lMixers[animEntry], wantedFadeDuration);
                        break;
                }

                if (avatarMask != group.Value.layers[layer].Mask) group.Value.layers[layer].Mask = avatarMask;
            }
        }

        private float GetFadeTimeFor(AssetRef<AnimationEntry> lastEntry, AssetRef<AnimationEntry> entry)
        {
            if (!QuantumUnityDB.TryGetGlobalAsset(lastEntry, out var lastEntryAsset))
                return 0;
            return lastEntryAsset.GetFade(entry);
        }

        private void SetLayerWeight(int layer, float weight)
        {
            foreach (var group in animatorInfoGroups)
            {
                group.Value.layers[layer]?.SetWeight(weight);
            }
        }

        private void UpdateMixers(int layer, Vector2 mixerParameter)
        {
            foreach (var group in animatorInfoGroups)
            {
                switch (group.Value.layerMixerType[layer])
                {
                    case AnimationEntry.MixerType.Linear:
                        (group.Value.states[layer] as LinearMixerState).Parameter = mixerParameter.x;
                        break;
                    case AnimationEntry.MixerType.Cartesian:
                        (group.Value.states[layer] as CartesianMixerState).Parameter = mixerParameter;
                        break;
                    case AnimationEntry.MixerType.Directional:
                        (group.Value.states[layer] as DirectionalMixerState).Parameter = mixerParameter;
                        break;
                }
            }
        }

        private void PauseAnimators()
        {
            foreach (var group in animatorInfoGroups)
            {
                if (group.Value.animancer.Graph.IsGraphPlaying) group.Value.animancer.Graph.PauseGraph();
            }
        }

        private void EvaluateAnimators(float deltaTime)
        {
            foreach (var group in animatorInfoGroups)
            {
                group.Value.animancer.Evaluate(deltaTime);
            }
        }
    }
}
#endif