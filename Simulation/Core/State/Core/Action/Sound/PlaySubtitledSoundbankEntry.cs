using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class PlaySubtitledSoundbankEntry : HNSFStateAction
    {
        public AssetRef<Tag> voiceClip;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            //frame.Events.PlaySubtitledSoundbankEntry(infoEntityRef, voiceClip, voiceClip, 1);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new PlaySubtitledSoundbankEntry());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as PlaySubtitledSoundbankEntry;
            t.voiceClip = voiceClip;
            return base.CopyTo(target);
        }
    }
}