using System;
using Quantum;
using Quantum.Physics2D;
using HnSF.core.AI.HTN.Conditions;
using HnSF.core.AI.HTN.Param;
using HnSF.Nodes;
using Photon.Deterministic;
using UnityEngine.Serialization;
#if QUANTUM_UNITY
using UnityEngine;
#endif
#if UNITY_EDITOR
using HnSF.core.GroupControl.Nodes;
using Unity.GraphToolkit.Editor;
#endif

namespace HnSF.core.AI.HTN.Conditions
{
    [Serializable]
    public unsafe partial class RandomChance : ICondition
    {
        [field: SerializeField] public string Label { get; set; } = "";
        
        public HTNParamInt chanceParam;
        
        public bool IsValid(ref HTNAgentContext context)
        {
            return context.frame.RNG->NextInclusive(1, 100) <= chanceParam.Resolve(ref context);
        }
    }
}

# if UNITY_EDITOR
namespace HnSF.core.AI.HTN.Nodes
{
    [Serializable]
    internal class ConditionRandomChance : ConditionBase
    {
        public const string inChance = "MinParam";
        
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            base.OnDefinePorts(context);

            context.AddInputPort(inChance)
                .WithDisplayName("Chance (0-100)")
                .Build();
        }

        public override ICondition Convert()
        {
            //this.GetNodeOptionByName(OPTION_LABEL).TryGetValue<string>(out var label);
            
            return new Conditions.RandomChance()
            {
                chanceParam = NodeHelper.GetInputPortParam<HTNParamInt, int>(GetInputPortByName(inChance))
            };
        }
    }
}
#endif