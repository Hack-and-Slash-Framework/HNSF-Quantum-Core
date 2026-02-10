namespace Quantum
{
    public unsafe partial struct IsBeingThrown
    {
        public static void EscapeThrow(Frame frame, EntityRef entityRef)
        {
            var isInThrow = frame.Get<IsBeingThrown>(entityRef);

            var thrower = frame.Unsafe.GetPointer<IsThrowing>(isInThrow.thrower);
            thrower->ReleaseThrowee(frame, isInThrow.thrower, entityRef);
        }
    }
}