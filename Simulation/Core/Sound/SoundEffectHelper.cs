using Photon.Deterministic;

namespace Quantum
{
    public static unsafe partial class SoundEffectHelper
    {
        public static bool PlaySound(Frame frame, PlaySoundRequest request, EntityRef entity, FPVector3 position)
        {
            var sound = request.GetRngSound(frame.RNG);
            if (!sound.soundRef.IsValid) return false;
            PlaySound(frame, request, sound, entity, position);
            return true;
        }

        public static void PlaySound(Frame frame, PlaySoundRequest request, PlaySoundRequest.SoundReference sound,
            EntityRef entity, FPVector3 position)
        {
            frame.Events.PlaySoundAtLocation(
                owner: entity,
                parentedTo: request.parentedToSelf ? entity : EntityRef.None,
                sound: sound.soundRef,
                volume: sound.volume,
                minPitch: sound.minPitch,
                maxPitch: sound.maxPitch,
                minDistance: request.minDistance,
                maxDistance: request.maxDistance,
                cancelOthersSoundEntry: request.cancelSameSound,
                cancelOthersTag: request.cancelSameTag,
                ignoreIfSoundPlaying: request.ignoreIfSoundPlaying,
                ignoreIfTagPlaying: request.ignoreIfTagPlaying,
                isGlobal: request.isGlobal,
                audioSourceConfig: request.audioSourceConfig,
                position: position,
                tag: request.tag
            );
        }
    }
}