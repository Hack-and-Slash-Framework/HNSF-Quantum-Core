using Quantum;

namespace HnSF.core.systems
{
    public class ExternalSystemRegistrySystem : SystemSignalsOnly, ISignalOnComponentAdded<ExternalSystemRegistry>, ISignalOnComponentRemoved<ExternalSystemRegistry>
    {
        public unsafe void OnAdded(Frame f, EntityRef entity, ExternalSystemRegistry* component)
        {
            component->systemGroups = f.AllocateDictionary<int, ExternalSystemGroup>();
        }

        public unsafe void OnRemoved(Frame f, EntityRef entity, ExternalSystemRegistry* component)
        {
            var dict = f.ResolveDictionary(component->systemGroups);

            foreach (var k in dict)
            {
                f.FreeList(k.Value.systems);
            }
            dict.Clear();
            
            f.FreeDictionary(ref component->systemGroups);
        }
    }
}