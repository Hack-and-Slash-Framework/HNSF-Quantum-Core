using System;
using System.Collections.Generic;
using Photon.Deterministic;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    public partial class HurtboxInfo : AssetObject
    {
        public List<AssetRef<Tag>> invincibleAgainstAttributes = new();
        [NonSerialized] public HashSet<AssetRef<Tag>> invincibleAgainstAttributesHashSet = new();
        
#if QUANTUM_UNITY
        [Header("Armor")]
#endif
        public bool armor = false;
        [DrawIf(nameof(armor), true)]
        public StandardHitReactions armorAttackerReturnedReaction = StandardHitReactions.Blocked;
        [DrawIf(nameof(armor), true)]
        public StandardHitReactions armorDefenderReturnedReaction = StandardHitReactions.Blocked;
        [DrawIf(nameof(armor), true)]
        public List<AssetRef<Tag>> canBeBrokenBy = new();
        [NonSerialized] public HashSet<AssetRef<Tag>> armorCanBeBrokenByHashSet = new();
        [DrawIf(nameof(armor), true)]
        public bool takeDamage;
        [DrawIf(nameof(armor), true)]
        public bool brokenByDamage;
        [DrawIf(nameof(armor), true)]
        [DrawIf(nameof(brokenByDamage), true)]
        public int damageToBreakArmor;

        public override void Loaded(IResourceManager resourceManager, Native.Allocator allocator)
        {
            base.Loaded(resourceManager, allocator);
            foreach (var t in invincibleAgainstAttributes) invincibleAgainstAttributesHashSet.Add(t);
            foreach(var t in canBeBrokenBy) armorCanBeBrokenByHashSet.Add(t);
        }
    }
}
