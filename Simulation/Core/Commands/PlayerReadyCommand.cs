using Photon.Deterministic;

namespace Quantum
{
    public class PlayerReadyCommand : DeterministicCommand
    {
        public int readyType;
        
        public override void Serialize(BitStream stream)
        {
            stream.Serialize(ref readyType);
        }
    }
}