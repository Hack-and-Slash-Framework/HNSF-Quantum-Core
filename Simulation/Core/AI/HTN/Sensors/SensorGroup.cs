using System.Collections.Generic;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF.core.AI.HTN.Sensors
{
    public unsafe class SensorGroup : AssetObject
    {
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public List<ISensor> sensors = new List<ISensor>();
        
        public virtual void Execute(ref HTNAgentContext context, bool executingOnly = true)
        {
            if (executingOnly && context.agent->contextState != HTNContextState.Executing)
                return;
            
            foreach (var sensor in sensors)
            {
                sensor.Execute(ref context);
            }
        }
    }
}
