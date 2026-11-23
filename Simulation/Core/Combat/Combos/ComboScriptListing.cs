using System;
using HnSF.core.GroupControl.Combo;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF
{
    public class ComboScriptListing : AssetObject
    {
#if QUANTUM_UNITY
        [Header("General")]
#endif
        public AssetRef<Tag> tag;
        public AssetRef<Tag>[] infoTags = Array.Empty<AssetRef<Tag>>();
        public AssetRef<ComboBattleScript>[] comboScripts = Array.Empty<AssetRef<ComboBattleScript>>();
    }
}
