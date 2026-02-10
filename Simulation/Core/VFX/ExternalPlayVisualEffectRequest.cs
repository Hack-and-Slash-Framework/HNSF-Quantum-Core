using System;

namespace Quantum
{
    [System.Serializable]
    public class ExternalPlayVisualEffectRequest : AssetObject
    {
        public PlayVisualEffectRequest request;
        
        public ExternalPlayVisualEffectRequest()
        {
            request.visualEffects = Array.Empty<PlayVisualEffectRequest.VFXReference>();
            request.chance = 1;
        }
    }
}