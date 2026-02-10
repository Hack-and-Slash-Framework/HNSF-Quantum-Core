using Photon.Deterministic;
using Quantum;

namespace HnSF.core
{
    public class SpawnBattleActorCommand : DeterministicCommand
    {
        public int id;
        public AssetRef<BattleActorDefinition> battleActorDefinition;
        
        public override void Serialize(BitStream stream)
        {
            stream.Serialize(ref id);
            stream.Serialize(ref battleActorDefinition);
        }
    }
}