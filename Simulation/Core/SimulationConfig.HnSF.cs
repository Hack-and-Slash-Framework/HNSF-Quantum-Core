#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    public partial class SimulationConfig : AssetObject
    {
#if QUANTUM_UNITY
        [Header("Layers")]
#endif
        public LayerMask layerMaskWarningbox;
        public Quantum.LayerMask layerMaskHitbox;
        public Quantum.LayerMask layerMaskHurtbox;
        public Quantum.LayerMask layerMaskCollisionbox;
        public Quantum.LayerMask layerMaskThrowbox;
        
#if QUANTUM_UNITY
        [Header("Tags")]
#endif
        public AssetRef<Tag> tag_AttackAttribute_Strike_Head;
        public AssetRef<Tag> tag_AttackAttribute_Strike_Body;
        public AssetRef<Tag> tag_AttackAttribute_Strike_Foot;
        public AssetRef<Tag> tag_AttackAttribute_Throw;
        public AssetRef<Tag> tag_AttackAttribute_Projectile;
        public AssetRef<Tag> tag_AttackAttribute_Puppet;
        public AssetRef<Tag> tag_AttackAttribute_Burst;

        public AssetRef<Tag> tag_EventHandler_ActorGlobal;
        public AssetRef<Tag> tag_self;
        
        public AssetRef<Tag> stateType_Idle;
        public AssetRef<Tag> stateType_Attack;
        public AssetRef<Tag> stateType_Hitstun;

        public AssetRef<Tag> stateInfo_Cutscene;
        public AssetRef<Tag> stateInfo_Special;
        
        public AssetRef<Tag> stateTag_MatchInit;
        public AssetRef<Tag> stateTag_MatchIntro;
        public AssetRef<Tag> stateTag_IntroWait;
        public AssetRef<Tag> stateTag_Stand;
        public AssetRef<Tag> stateTag_Fall;
        public AssetRef<Tag> stateTag_VictoryScreen;
    }
}