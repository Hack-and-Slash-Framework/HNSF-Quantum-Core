using System.Collections.Generic;
using Quantum;

namespace HnSF
{
    public static partial class MapRegister
    {
        public static readonly Dictionary<AssetRef<Quantum.Map>, IMapDefinition> QuantumMapToMapDefinition = new();
        public static readonly Dictionary<IMapDefinition, AssetRef<Quantum.Map>> MapDefinitionToQuantumMap = new();

        public static void Clear()
        {
            QuantumMapToMapDefinition.Clear();
            MapDefinitionToQuantumMap.Clear();
        }
        
        public static void Register(IMapDefinition mapDefinition, AssetRef<Quantum.Map> quantumMapRef)
        {
            QuantumMapToMapDefinition[quantumMapRef] = mapDefinition;
            MapDefinitionToQuantumMap[mapDefinition] = quantumMapRef;
        }

        public static void Deregister(IMapDefinition mapDefinition, AssetRef<Quantum.Map> quantumMapRef)
        {
            MapDefinitionToQuantumMap.Remove(mapDefinition);
            QuantumMapToMapDefinition.Remove(quantumMapRef);
        }
    }
}