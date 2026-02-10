using System;

namespace Quantum
{
    [System.Serializable]
    public class LayerMaskParam
    {
        public enum LayerMaskSourceType
        {
            Self,
            External
        }

        public LayerMaskSourceType source = LayerMaskSourceType.Self;

        [DrawIf(nameof(source), (int)LayerMaskSourceType.Self)]
        public LayerMask layerMask;

        [DrawIf(nameof(source), (int)LayerMaskSourceType.External)]
        public AssetRef<ExternalLayerMask> externalLayerMask;

        [NonSerialized] private ExternalLayerMask _externalLayerMask = null;

        public LayerMask Get(Frame frame)
        {
            switch (source)
            {
                case LayerMaskSourceType.External:
                    if (_externalLayerMask is null && (externalLayerMask.IsValid == false ||
                                                       !frame.TryFindAsset(externalLayerMask, out _externalLayerMask)))
                        break;
                    return _externalLayerMask.mask;
            }
            return layerMask;
        }

        public virtual LayerMaskParam Clone()
        {
            return new LayerMaskParam()
            {
                source = source,
                layerMask = layerMask,
                externalLayerMask = externalLayerMask
            };
        }
    }
}