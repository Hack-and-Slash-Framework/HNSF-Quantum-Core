using System;

namespace Quantum
{
    [System.Serializable]
    public class ExternalPlaySoundRequest : AssetObject
    {
        public PlaySoundRequest request;

        public ExternalPlaySoundRequest()
        {
            request.sounds = Array.Empty<PlaySoundRequest.SoundReference>();
            request.minDistance = 0;
            request.maxDistance = 5;
            request.chance = 1;
        }
    }
}
