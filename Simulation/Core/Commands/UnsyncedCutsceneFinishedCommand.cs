using Photon.Deterministic;

namespace Quantum
{
    public class UnsyncedCutsceneFinishedCommand : DeterministicCommand
    {
        public AssetRef<Tag> cutsceneSourceTag;
        public AssetRef<Tag> cutsceneTag;
        
        public override void Serialize(BitStream stream)
        {
            stream.Serialize(ref cutsceneSourceTag);
            stream.Serialize(ref cutsceneTag);
        }
    }
}