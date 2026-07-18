namespace Quantum
{
    public unsafe partial struct PlayerLink
    {
        public static bool TryGetPlayerLink(Frame frame, EntityRef entityRef, out PlayerLink* playerLink)
        {
            playerLink = null;
            return frame.Exists(entityRef) && frame.Unsafe.TryGetPointer<PlayerLink>(entityRef, out playerLink);
        }
        
        public static bool TryGetPlayerRef(Frame frame, EntityRef entityRef, out PlayerRef playerRef)
        {
            playerRef = default;
            if (!frame.Exists(entityRef))
                return false;
            if (!frame.Unsafe.TryGetPointer<PlayerLink>(entityRef, out var pl))
                return false;
            playerRef = pl->Player;
            return true;
        }
    }
}
