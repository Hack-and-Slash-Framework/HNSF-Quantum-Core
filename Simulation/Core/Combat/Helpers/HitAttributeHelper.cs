using System.Collections.Generic;

namespace Quantum
{
    public static unsafe partial class HitAttributeHelper
    {
        public static bool IsHurtboxAttributeInvincible(SimulationConfig simulationConfig, List<AssetRef<Tag>> hurtboxInvincibleAgainstAttributes, List<AssetRef<Tag>> hitboxAttributes)
        {
            if (hitboxAttributes.Count == 0 || hurtboxInvincibleAgainstAttributes.Count == 0) return false;
            
            if (hitboxAttributes.Contains(simulationConfig.tag_AttackAttribute_Burst)) return false;

            bool containsAnyStrike = hitboxAttributes.Contains(simulationConfig.tag_AttackAttribute_Strike_Body) ||
                                     hitboxAttributes.Contains(simulationConfig.tag_AttackAttribute_Strike_Head) ||
                                     hitboxAttributes.Contains(simulationConfig.tag_AttackAttribute_Strike_Foot);
            
            // Hitbox only needs a single attribute that the hurtbox doesn't have to hit.
            for (int i = 0; i < hitboxAttributes.Count; i++)
            {
                if (hitboxAttributes[i] == simulationConfig.tag_AttackAttribute_Projectile
                    && containsAnyStrike)
                {
                    continue;
                }
                
                if (!hurtboxInvincibleAgainstAttributes.Contains(hitboxAttributes[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool CanHitboxesClashBasedOnAttributes(SimulationConfig simulationConfig, List<AssetRef<Tag>> hitboxAAttributes, List<AssetRef<Tag>> hitboxBAttributes)
        {
            bool aContainsAnyStrike = hitboxAAttributes.Contains(simulationConfig.tag_AttackAttribute_Strike_Body) ||
                                      hitboxAAttributes.Contains(simulationConfig.tag_AttackAttribute_Strike_Head) ||
                                      hitboxAAttributes.Contains(simulationConfig.tag_AttackAttribute_Strike_Foot);
            
            bool bContainsAnyStrike = hitboxBAttributes.Contains(simulationConfig.tag_AttackAttribute_Strike_Body) ||
                                      hitboxBAttributes.Contains(simulationConfig.tag_AttackAttribute_Strike_Head) ||
                                      hitboxBAttributes.Contains(simulationConfig.tag_AttackAttribute_Strike_Foot);

            return aContainsAnyStrike && bContainsAnyStrike;
        }
    }
}
