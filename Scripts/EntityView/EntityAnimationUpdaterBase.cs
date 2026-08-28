using System;
using System.Collections.Generic;
using Quantum;
using UnityEngine;

namespace HnSF
{
    public unsafe class EntityAnimationUpdaterBase : MonoBehaviour, IOnUModPrebuild, IEditorAwake
    {
        [System.Serializable]
        public class TagAvatarMaskInfo
        {
            public AssetRef<Tag> tag;
            public AvatarMask avatarMask;
        }

        public QuantumEntityView entityView;
        public GameObjectRendererList renderersList;
        public GameObject modelRoot;

        protected List<DispatcherSubscription> _updateViewDispatchers = new List<DispatcherSubscription>();
        public bool disabled;
        [NonSerialized] public float updateRateTimer = 0;

        // State
        protected BattleActorAnimatorState _lastSetState;
        protected BattleActorAnimator actorAnimatorLast;
        protected BattleActorAnimator actorAnimator;
        protected int lastFrameUpdateNumber = -1;
        protected float accumulatedTimeSinceLastUpdate;

        // Masking Support
        [Header("Masking")] public TagAvatarMaskInfo[] tagAvatarMaskMapping = Array.Empty<TagAvatarMaskInfo>();
        [NonSerialized] public Dictionary<AssetRef<Tag>, AvatarMask> tagToAvatarMaskMapping = new();

        public virtual void OnUModPrebuild()
        {
        }

        public virtual void Awake()
        {
            tagToAvatarMaskMapping.Clear();
            foreach (var b in tagAvatarMaskMapping)
            {
                tagToAvatarMaskMapping.Add(b.tag, b.avatarMask);
            }

            entityView.OnEntityInstantiated.AddListener(WhenEntityInstantiated);
            entityView.OnEntityDestroyed.AddListener(WhenEntityDestroyed);
        }

        public virtual void OnDestroy()
        {
            Cleanup();
        }

        protected virtual void Cleanup()
        {
            EntityAnimationGlobalUpdaterBase.UnregisterAnimator(this);
        }

        protected virtual void WhenEntityInstantiated(QuantumGame arg0)
        {
            Reset();
            var priorityEntity =
                arg0.Frames.Predicted.Unsafe.TryGetPointer<PlayerLink>(entityView.EntityRef, out var pr) &&
                arg0.PlayerIsLocal(pr->Player);
            EntityAnimationGlobalUpdaterBase.RegisterAnimator(this, priorityEntity);
        }

        protected virtual void WhenEntityDestroyed(QuantumGame arg0)
        {
            Cleanup();
            Reset();
        }

        public virtual void Reset()
        {
            _lastSetState = default;
            lastFrameUpdateNumber = -1;
            actorAnimatorLast = default;
            actorAnimator = default;
            accumulatedTimeSinceLastUpdate = 0;
        }

        public virtual void UpdateAnimatorState(QuantumGame game)
        {
        }
    }
}