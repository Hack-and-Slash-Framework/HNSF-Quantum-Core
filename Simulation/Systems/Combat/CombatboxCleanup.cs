using Quantum;

namespace HnSF.Systems
{
    public class CombatboxCleanup : SystemSignalsOnly, ISignalOnComponentRemoved<BoxCombatant>
    {
        public unsafe void OnRemoved(Frame frame, EntityRef entity, BoxCombatant* component)
        {
            BoxCombatantHelper.CleanupAllBoxes(frame, component);
        }
    }
}
