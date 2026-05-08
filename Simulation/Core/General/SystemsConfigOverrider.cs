using System;
using System.Collections.Generic;
using System.Linq;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    [Serializable]
#if QUANTUM_UNITY
    [UnityEngine.CreateAssetMenu(menuName = "Quantum/Configurations/SystemsConfigOverrider", order = -897)]
#endif
    public class SystemsConfigOverrider : AssetObject
    {
        [Serializable]
        public class BeforeAfterConfigGroup
        {
            public string Label;
            public SerializableType<SystemBase>[] addBeforeSystems;
            public SerializableType<SystemBase>[] addAfterSystems;
            public SystemsConfig.SystemEntry[] systemsToAdd;
        }
        
        public SystemsConfig baseSystemsConfig;

        public BeforeAfterConfigGroup[] configGroups;

        public SystemsConfig BuildSystemsConfig()
        {
            var sc = ScriptableObject.CreateInstance<SystemsConfig>();
            sc.Entries.Clear();
            if (baseSystemsConfig == null)
            {
                return sc;
            }
            foreach (var baseEntry in baseSystemsConfig.Entries)
            {
                _AddSystem(sc, baseEntry);
            }

            foreach (var configGroup in configGroups)
            {
                _AddExtraSystems(sc,configGroup);
            }
            
            return sc;
        }
        
        private static void _AddSystem(SystemsConfig sc, SystemsConfig.SystemEntry baseEntry)
        {
            var entry = new SystemsConfig.SystemEntry()
            {
                SystemType = baseEntry.SystemType,
                StartDisabled = baseEntry.StartDisabled
            };
            
            sc.Entries.Add(entry);

            entry.Children = new List<SystemsConfig.SubSystemEntry>(baseEntry.Children.Count);
            
            // TODO: Children handling.
        }
        
        private static void _AddExtraSystems(SystemsConfig sc, BeforeAfterConfigGroup extraConfig)
        {
            var shouldBeAfter = extraConfig.addAfterSystems;
            var shouldBeAfterIndexes = new int[shouldBeAfter.Length];
            var shouldBeBefore = extraConfig.addBeforeSystems;
            var shouldBeBeforeIndexes = new int[shouldBeBefore.Length];

            if (shouldBeAfter.Length == 0 && shouldBeBefore.Length == 0)
            {
                for (int i = 0; i < extraConfig.systemsToAdd.Length; i++)
                {
                    sc.Entries.Insert(sc.Entries.Count, new SystemsConfig.SystemEntry()
                    {
                        SystemType = extraConfig.systemsToAdd[i].SystemType.Value,
                        StartDisabled = extraConfig.systemsToAdd[i].StartDisabled
                    }); // TODO: Children support.
                }
                return;
            }
            
            for (int i = 0; i < sc.Entries.Count; i++)
            {
                var afterIndex = Array.IndexOf(shouldBeAfter, sc.Entries[i].SystemType.Value);
                var beforeIndex = Array.IndexOf(shouldBeBefore, sc.Entries[i].SystemType.Value);

                if (afterIndex >= 0) shouldBeAfterIndexes[afterIndex] = i;
                if(beforeIndex >= 0) shouldBeBeforeIndexes[beforeIndex] = i;
            }
            
            // After Only
            if (shouldBeAfterIndexes.Length > 0 && shouldBeBeforeIndexes.Length == 0)
            {
                var neededIndex = shouldBeAfterIndexes.Max()+1;
                for (int i = extraConfig.systemsToAdd.Length - 1; i >= 0; i--)
                {
                    sc.Entries.Insert(neededIndex, new SystemsConfig.SystemEntry()
                    {
                        SystemType = extraConfig.systemsToAdd[i].SystemType.Value,
                        StartDisabled = extraConfig.systemsToAdd[i].StartDisabled
                    }); // TODO: Children support.
                }
            } // Before Only
            else if (shouldBeBeforeIndexes.Length > 0 && shouldBeAfterIndexes.Length == 0)
            {
                var neededIndex = shouldBeBeforeIndexes.Min();

                for (int i = extraConfig.systemsToAdd.Length - 1; i >= 0; i--)
                {
                    sc.Entries.Insert(neededIndex, new SystemsConfig.SystemEntry()
                    {
                        SystemType = extraConfig.systemsToAdd[i].SystemType.Value,
                        StartDisabled = extraConfig.systemsToAdd[i].StartDisabled
                    }); // TODO: Children support.
                }
            } // Between Before & After
            else if (shouldBeBeforeIndexes.Length > 0 && shouldBeAfterIndexes.Length > 0)
            {
                var neededIndex = shouldBeAfterIndexes.Max()+1;
                var minBefore = shouldBeBeforeIndexes.Min();

                if (neededIndex > minBefore)
                {
                    Debug.LogError($"Can't place system properly due to system ordering. Skipping.");
                    return;
                }

                for (int i = extraConfig.systemsToAdd.Length - 1; i >= 0; i--)
                {
                    sc.Entries.Insert(neededIndex, new SystemsConfig.SystemEntry()
                    {
                        SystemType = extraConfig.systemsToAdd[i].SystemType.Value,
                        StartDisabled = extraConfig.systemsToAdd[i].StartDisabled
                    }); // TODO: Children support.
                }
            }
        }
    }
}
