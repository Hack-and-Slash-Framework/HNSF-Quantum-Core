namespace Quantum
{
    [System.Serializable]
    public unsafe partial struct PlayVisualEffectRequestParam
    {
        [System.Serializable]
        public enum ParamType
        {
            Self,
            External
        }

        public ParamType type;
        public PlayVisualEffectRequest request;
        public AssetRef<ExternalPlayVisualEffectRequest> externalRequest;
        
        public PlayVisualEffectRequest Resolve(Frame frame)
        {
            switch (type)
            {
                case ParamType.Self:
                    return request;
                case ParamType.External:
                    if (frame.TryFindAsset(externalRequest, out var externalRequestAsset))
                        return externalRequestAsset.request;
                    break;
            }
            return default;
        }

        public PlayVisualEffectRequestParam Clone()
        {
            var clone = new PlayVisualEffectRequestParam
            {
                type = type,
                request = request.Clone(),
                externalRequest = externalRequest
            };
            return clone;
        }
    }
}