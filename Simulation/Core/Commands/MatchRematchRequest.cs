using Photon.Deterministic;

namespace Quantum
{
    public class MatchRematchRequest : DeterministicCommand
    {
        public int request;
        
        public override void Serialize(BitStream stream)
        {
            stream.Serialize(ref request);
        }
    }
}