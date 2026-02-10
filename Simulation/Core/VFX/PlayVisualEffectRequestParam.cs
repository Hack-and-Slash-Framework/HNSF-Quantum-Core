using System;

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
        
        [NonSerialized] private ExternalPlayVisualEffectRequest _externalRequest;

        public PlayVisualEffectRequest Resolve(Frame frame)
        {
            switch (type)
            {
                case ParamType.Self:
                    return request;
                case ParamType.External:
                    if (_externalRequest == null && !frame.TryFindAsset(externalRequest, out _externalRequest))
                        return default;
                    return _externalRequest.request;
            }
            return default;
        }

        public void ClearCache()
        {
            _externalRequest = null;
        }
    }
}