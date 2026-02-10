using Quantum;

namespace HnSF.core.state.functions
{
    [System.Serializable]
    public unsafe partial class StateFunctionAssetRef : HNSFStateFunction<AssetRef>
    {
        public override AssetRef Execute(Frame frame, EntityRef entity, ref HNSFStateContext stateContext)
        {
            return default;
        }
    }
}