using System;
using Photon.Deterministic;
using Quantum;

namespace HnSF.core.state.actions
{
    [Serializable]
    public unsafe partial class PlaySubtitledSoundEntry : HNSFStateAction
    {
        public AssetRef<SoundEntry> voiceClip;
        public FP volume = 1;
        
        public override bool ExecuteAction(Frame frame, EntityRef entity, FP rangePercent,
            ref HNSFStateContext stateContext)
        {
            var targetEntityRef = GetActionTargetEntityRef(frame, entity);
            if (targetEntityRef == EntityRef.None) return false;
            
            frame.Events.PlaySubtitledSoundEntry(targetEntityRef, voiceClip, volume);
            return false;
        }

        public override HNSFStateAction Copy()
        {
            return CopyTo(new PlaySubtitledSoundEntry());
        }

        public override HNSFStateAction CopyTo(HNSFStateAction target)
        {
            var t = target as PlaySubtitledSoundEntry;
            t.voiceClip = voiceClip;
            t.volume = volume;
            return base.CopyTo(target);
        }
    }
}