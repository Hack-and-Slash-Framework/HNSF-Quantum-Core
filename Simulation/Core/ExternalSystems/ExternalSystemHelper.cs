namespace Quantum
{
    public static unsafe partial class ExternalSystemHelper
    {
        public static void CreateExternalSystemGroup(Frame frame, int groupId)
        {
            var externalSystemRegistry = frame.GetOrAddSingleton<ExternalSystemRegistry>();
            var externalSystemGroupDictionary = frame.ResolveDictionary(externalSystemRegistry.systemGroups);
            if (externalSystemGroupDictionary.ContainsKey(groupId)) return;

            var group = new ExternalSystemGroup();
            group.systems = frame.AllocateList<ExternalSystemRefCounted>();
            
            externalSystemGroupDictionary.Add(groupId, group);
        }

        public static void CallExternalSystemGroup(Frame f, int groupId)
        {
            var externalSystemRegistry = f.GetOrAddSingleton<ExternalSystemRegistry>();
            var externalSystemGroupDictionary = f.ResolveDictionary(externalSystemRegistry.systemGroups);
            if (!externalSystemGroupDictionary.ContainsKey(groupId)) return;

            var sys = f.ResolveList(externalSystemGroupDictionary[groupId].systems);

            foreach (var exs in sys)
            {
                if (!f.TryFindAsset(exs.externalSystem, out var externalSystem)) continue;
                externalSystem.Execute(f);
            }
        }
    }
}
