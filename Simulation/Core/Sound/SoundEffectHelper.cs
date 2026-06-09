using Photon.Deterministic;

namespace Quantum
{
    public static unsafe partial class SoundEffectHelper
    {
        public static bool PlaySound(Frame frame, PlaySoundRequest request, EntityRef entity, FPVector3 position,
            EntityRef? owner = null,
            EntityRef? parentedTo = null,
            FP? volume = null,
            FP? minPitch = null,
            FP? maxPitch = null,
            FP? minDistance = null,
            FP? maxDistance = null,
            bool? cancelOthersSoundEntry = null,
            bool? cancelOthersTag = null,
            bool? ignoreIfSoundPlaying = null,
            bool? ignoreIfTagPlaying = null,
            bool? isGlobal = null,
            AssetRef<AudioSourceConfig>? audioSourceConfig = null,
            FPVector3? positionOverride = null,
            AssetRef<Tag>? tag = null)
        {
            var sound = request.GetRngSound(frame.RNG);
            if (!sound.soundRef.IsValid) return false;
            PlaySound(
                frame,
                request,
                sound,
                entity,
                position,
                owner,
                parentedTo,
                volume,
                minPitch,
                maxPitch,
                minDistance,
                maxDistance,
                cancelOthersSoundEntry,
                cancelOthersTag,
                ignoreIfSoundPlaying,
                ignoreIfTagPlaying,
                isGlobal,
                audioSourceConfig,
                positionOverride,
                tag);
            return true;
        }

        public static void PlaySound(Frame frame, PlaySoundRequest request, PlaySoundRequest.SoundReference sound,
            EntityRef entity, FPVector3 position,
            EntityRef? owner = null,
            EntityRef? parentedTo = null,
            FP? volume = null,
            FP? minPitch = null,
            FP? maxPitch = null,
            FP? minDistance = null,
            FP? maxDistance = null,
            bool? cancelOthersSoundEntry = null,
            bool? cancelOthersTag = null,
            bool? ignoreIfSoundPlaying = null,
            bool? ignoreIfTagPlaying = null,
            bool? isGlobal = null,
            AssetRef<AudioSourceConfig>? audioSourceConfig = null,
            FPVector3? positionOverride = null,
            AssetRef<Tag>? tag = null)
        {
            frame.Events.PlaySoundAtLocation(
                owner: owner ?? entity,
                parentedTo: parentedTo ?? (request.parentedToSelf ? entity : EntityRef.None),
                sound: sound.soundRef,
                volume: volume ?? sound.volume,
                minPitch: minPitch ?? sound.minPitch,
                maxPitch: maxPitch ?? sound.maxPitch,
                minDistance: minDistance ?? request.minDistance,
                maxDistance: maxDistance ?? request.maxDistance,
                cancelOthersSoundEntry: cancelOthersSoundEntry ?? request.cancelSameSound,
                cancelOthersTag: cancelOthersTag ?? request.cancelSameTag,
                ignoreIfSoundPlaying: ignoreIfSoundPlaying ?? request.ignoreIfSoundPlaying,
                ignoreIfTagPlaying: ignoreIfTagPlaying ?? request.ignoreIfTagPlaying,
                isGlobal: isGlobal ?? request.isGlobal,
                audioSourceConfig: audioSourceConfig ?? request.audioSourceConfig,
                position: positionOverride ?? position,
                tag: tag ?? request.tag
            );
        }
    }
}