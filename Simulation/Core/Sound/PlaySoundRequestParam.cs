using System;

namespace Quantum
{
    [System.Serializable]
    public unsafe partial struct PlaySoundRequestParam
    {
        [System.Serializable]
        public enum ParamType
        {
            Self,
            External
        }

        public ParamType type;
        public PlaySoundRequest request;
        public AssetRef<ExternalPlaySoundRequest> externalRequest;
        
#if UNITY_EDITOR
        public bool editorRefresh;
#endif
        
        [NonSerialized] private ExternalPlaySoundRequest _externalRequest;
        
        public PlaySoundRequest Resolve(Frame frame)
        {
#if UNITY_EDITOR
            if (editorRefresh) _externalRequest = null;
#endif
            
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

        public void OnValidate()
        {
            if(type == ParamType.Self)
                request.OnValidate();
        }

        public PlaySoundRequestParam Clone()
        {
            var clone = new PlaySoundRequestParam()
            {
                type = type,
                request = request.Clone(),
                externalRequest = externalRequest
            };
            return clone;
        }
    }
}
