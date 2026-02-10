using System;

namespace Quantum
{
    [System.Serializable]
    public unsafe partial struct ScreenShakeRequestParam
    {
        [System.Serializable]
        public enum ParamType
        {
            Self,
            External
        }

        public ParamType type;
        public ScreenShakeRequest request;
        public AssetRef<ExternalScreenShakeRequest> externalRequest;
        
        [NonSerialized] private ExternalScreenShakeRequest _externalRequest;
        
        public ScreenShakeRequest Resolve(Frame frame)
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