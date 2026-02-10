using System;

namespace HnSF.core.state.actions
{
    [Serializable]
    [AddTypeMenu(menuName: "Empty")]
    public unsafe partial class Empty : HNSFStateAction
    {
        public override HNSFStateAction Copy()
        {
            return CopyTo(new Empty());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            return base.CopyTo(target);
        }
    }
}