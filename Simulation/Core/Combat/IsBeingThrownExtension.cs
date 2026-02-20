namespace Quantum
{
    public unsafe partial struct IsBeingThrown
    {
        public void ReleaseFromThrow(Frame frame, EntityRef selfEntityRef)
        {
            if (!frame.Unsafe.TryGetPointer<IsThrowing>(thrower, out var throwerComponent))
            {
                frame.Remove<IsBeingThrown>(selfEntityRef);
                Log.Warn("Throwee released from throw due to thrower not existing.");
                return;
            }
            throwerComponent->ReleaseThrowee(frame, thrower, selfEntityRef);
        }
    }
}