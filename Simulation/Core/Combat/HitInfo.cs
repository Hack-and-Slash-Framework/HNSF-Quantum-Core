using System;
using System.Collections.Generic;
using HnSF.core.state.functions;
using Photon.Deterministic;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    public partial class HitInfo : AssetObject
    {
#if QUANTUM_UNITY
        [Header("General")]
#endif
        public List<AssetRef<Tag>> attributes;
        public AssetRef<HitInfo> counterhitInfo;
        public bool blockedFromAnyAngle = false;
        public bool blockDirBasedOnRotation = false;
        public int hitCount = 1;
        
#if QUANTUM_UNITY
        [Header("Reaction")] [SerializeReference, SubclassSelector]
#endif
        public StateFunctionAssetRef groundReaction = new StateFunctionAssetRef();
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public StateFunctionAssetRef airReaction = new StateFunctionAssetRef();

        public bool hardKnockdown;
        public bool unblockable;
        public int clashLevel;
        public bool dontClash;
        public bool usedByThrow;
        public bool breaksThrow;
        public bool assignThroweeOnHit;
        public bool faceHitDirection = true;
        
#if QUANTUM_UNITY
        [Header("Damage")]
#endif
        public FP damage = 1;
        public FP chipDamage = FP._0_10;
        public bool doesNotKill;
        public bool ignoreProration;
        public FP firstHitProtation = 1;
        public FP comboProration = FP.FromRaw(60293); // 0.92
        public FP sameMoveComboProration = FP.FromRaw(60293);
        public FP bonusProration = 0;
        public bool multihitComboProrationIgnore = true;
        
#if QUANTUM_UNITY
        [Header("Combo Decay")]
#endif
        public int firstHitComboDecay = 0;
        public int comboDecay = 5;
        public int sameMoveComboDecay = 5;
        public bool multihitComboDecayIgnore = true;
        public bool ignoreLaunchProration;
        public bool ignoreHorizontalLaunchProration;
        public bool ignoreGravityProration;
        public bool decayMoveIsSendingInward;
        public int minimumHitstun;
        public int minimumUntech;
        
#if QUANTUM_UNITY
        [Header("Stun")]
#endif
        public int attackLevel = -1;
        public int attackerHitstop;
        public int hitstop;
        public int hitstun;
        public int untech;
        public int blockstun;
        public bool ignoreHitstunScaling;
        
#if QUANTUM_UNITY
        [Header("Force")]
#endif
        public bool forceIgnoreRotY = true;
        public bool basedOnLookVector = false;
        public FPVector3 hitForceGrounded;
        public FPVector3 hitForceAerial;
        public FPVector3 blockForceGrounded;
        public FPVector3 blockForceAerial;
        public bool hasCustomGravity;
        [DrawIf(nameof(hasCustomGravity), true)]
        public FP hitstunGravity = 0;
        public bool hasCustomTraction;
        [DrawIf(nameof(hasCustomTraction), true)]
        public FP hitstunTraction = 0;
        public bool hasCustomAirFriction;
        [DrawIf(nameof(hasCustomAirFriction), true)]
        public FP hitstunAirFriction = 0;
        public bool canReverseHit;
        
#if QUANTUM_UNITY
        [Header("Hit Reactions")]
#endif
        public bool groundBounces;
        [DrawIf(nameof(groundBounces), true)] public int groundBounceHitstun;
        [DrawIf(nameof(groundBounces), true)] public HitForceData groundBounceForces;
        public bool wallBounces;
        [DrawIf(nameof(wallBounces), true)] public int wallBounceHitstun;
        [DrawIf(nameof(wallBounces), true)] public HitForceData wallBounceForces;
        
#if QUANTUM_UNITY
        [Header("Auto Link")]
#endif
        public bool autolink;
        public bool autoLinkBox;
        public FP autolinkPercent;
        public FP autolinkBoxPercent = FP.FromRaw(13107);
        public FPVector3 autolinkBoxOffset;
        
#if QUANTUM_UNITY
        [Header("Camera")]
#endif
        public ScreenShakeRequestParam[] onHitScreenShakes = Array.Empty<ScreenShakeRequestParam>();
        public ScreenShakeRequestParam[] onBlockedScreenShakes = Array.Empty<ScreenShakeRequestParam>();
        
#if QUANTUM_UNITY
        [Header("SFX")]
#endif
        public PlaySoundRequestParam[] onHitSounds = Array.Empty<PlaySoundRequestParam>();
        public PlaySoundRequestParam[] onBlockedSounds = Array.Empty<PlaySoundRequestParam>();

        
#if QUANTUM_UNITY
        [Header("VFX")]
#endif
        public PlayVisualEffectRequestParam[] onHitEffects = Array.Empty<PlayVisualEffectRequestParam>();
        public PlayVisualEffectRequestParam[] onBlockedEffects = Array.Empty<PlayVisualEffectRequestParam>();
    }
}
