using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Quantum;
using UnityEngine;

namespace HnSF
{
    public static class SystemsConfigHelper
    {
        public static SystemsConfig BuildSystemsConfig(SystemsConfig baseConfig, params SystemsConfig[] extraConfigs)
        {
            var sc = ScriptableObject.CreateInstance<SystemsConfig>();

            Debug.Log("Base Systems.");
            foreach (var baseEntry in baseConfig.Entries)
            {
                _AddSystem(sc, baseEntry);
            }

            Debug.Log("Extra Systems.");
            foreach (var extraConfig in extraConfigs)
            {
                _AddExtraSystems(sc, extraConfig);
            }
            
            return sc;
        }

        public static void PrintSystemsConfig(SystemsConfig config)
        {
            string outp = "SYSTEMS CONFIG \n";
            foreach (var sys in config.Entries)
            {
                outp += $"{(sys.SystemType.Value != null ? sys.SystemType.Value.Name : sys.SystemType.AssemblyQualifiedName)}\n";
            }
            Debug.Log(outp);
        }

        private static void _AddExtraSystems(SystemsConfig sc, SystemsConfig extraConfig)
        {
            foreach (var sys in extraConfig.Entries)
            {
                var typeInfo = sys.SystemType.Value?.GetTypeInfo();
                if (typeInfo == null)
                {
                    Debug.LogError($"Type Info is null for {sys.SystemType.AssemblyQualifiedName}, skipping.");
                    continue;
                }
                
                var beforeAttribute =
                    typeInfo.GetCustomAttributes(typeof(SystemOrderBeforeAttribute), true).FirstOrDefault() as SystemOrderBeforeAttribute;
                var afterAttribute =
                    typeInfo.GetCustomAttributes(typeof(SystemOrderAfterAttribute), true).FirstOrDefault() as SystemOrderAfterAttribute;

                var shouldBeAfter = afterAttribute != null ? afterAttribute.AfterSystems : Type.EmptyTypes;
                var shouldBeAfterIndexes = new int[shouldBeAfter.Length];
                var shouldBeBefore = beforeAttribute != null ? beforeAttribute.BeforeSystems : Type.EmptyTypes;
                var shouldBeBeforeIndexes = new int[shouldBeBefore.Length];

                if (shouldBeAfter.Length == 0 && shouldBeBefore.Length == 0)
                {
                    continue;
                }
                
                for (int i = 0; i < sc.Entries.Count; i++)
                {
                    var afterIndex = Array.IndexOf(shouldBeAfter, sc.Entries[i].SystemType.Value);
                    var beforeIndex = Array.IndexOf(shouldBeBefore, sc.Entries[i].SystemType.Value);

                    if (afterIndex >= 0) shouldBeAfterIndexes[afterIndex] = i;
                    if(beforeIndex >= 0) shouldBeBeforeIndexes[beforeIndex] = i;
                }

                if (shouldBeAfterIndexes.Length > 0 && shouldBeBeforeIndexes.Length == 0)
                {
                    var neededIndex = shouldBeAfterIndexes.Max()+1;
                    sc.Entries.Insert(neededIndex, new SystemsConfig.SystemEntry()
                    {
                        SystemType = sys.SystemType,
                        StartDisabled = sys.StartDisabled
                    }); // TODO: Children support.
                }else if (shouldBeBeforeIndexes.Length > 0 && shouldBeAfterIndexes.Length == 0)
                {
                    var neededIndex = shouldBeBeforeIndexes.Min();
                    sc.Entries.Insert(neededIndex, new SystemsConfig.SystemEntry()
                    {
                        SystemType = sys.SystemType,
                        StartDisabled = sys.StartDisabled
                    }); // TODO: Children support.
                }else if (shouldBeBeforeIndexes.Length > 0 && shouldBeAfterIndexes.Length > 0)
                {
                    var neededIndex = shouldBeAfterIndexes.Max()+1;
                    var minBefore = shouldBeBeforeIndexes.Min();

                    if (neededIndex > minBefore)
                    {
                        Debug.LogError($"Can't place system properly due to system ordering. {sys.SystemType.AssemblyQualifiedName}. Skipping.");
                        continue;
                    }
                    
                    sc.Entries.Insert(neededIndex, new SystemsConfig.SystemEntry()
                    {
                        SystemType = sys.SystemType,
                        StartDisabled = sys.StartDisabled
                    }); // TODO: Children support.
                }
            }
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
    }
}