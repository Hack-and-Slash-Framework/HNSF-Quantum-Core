using Quantum;

namespace HnSF.core.systems
{
    public unsafe class CallExternalSystemsPreLocalDeltaTime : SystemMainThread
    {
        public override void Update(Frame f)
        {
            ExternalSystemHelper.CallExternalSystemGroup(f, (int)ExternalSystemPlacements.PreLocalDeltaTime);
        }
    }
}