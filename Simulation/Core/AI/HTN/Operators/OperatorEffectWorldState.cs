using System;
using System.Collections.Generic;
using HnSF.core.AI.HTN.Functions;
using HnSF.core.AI.HTN.Param;
using HnSF.Nodes;
using Quantum;
using UnityEngine;
#if QUANTUM_UNITY
using UnityEngine.Scripting.APIUpdating;
#endif
#if UNITY_EDITOR
using HnSF.core.AI.HTN.Effects;
using HnSF.core.AI.HTN.Operators;
using Unity.GraphToolkit.Editor;
#endif

namespace HnSF.core.AI.HTN.Operators
{
    [Serializable]
    public unsafe partial class OperatorEffectWorldState : HTNOperatorBase
    {
        [Serializable]
        public class EffectPair
        {
            public EffectType effectType;
            public HTNParamByte stateID;
            public HTNParamByte stateValue;
        }
        
        public bool dirtyWorldState = true;
        public List<EffectPair> effectsList = new List<EffectPair>();
        
        public override HTNTaskStatus OnEnter(ref HTNAgentContext context)
        {
            var worldState = context.frame.ResolveDictionary(context.agent->worldState.current);
            
            foreach (var pair in effectsList)
            {
                HTNWorldState.SetState(
                    context: ref context,
                    state: pair.stateID.Resolve(ref context),
                    value: pair.stateValue.Resolve(ref context),
                    setAsDirty: dirtyWorldState,
                    e: pair.effectType,
                    worldState: ref worldState
                );
            }
            
            return HTNTaskStatus.Success;
        }

        public override HTNTaskStatus Tick(ref HTNAgentContext context)
        {
            return HTNTaskStatus.Success;
        }

        public override void OnExit(ref HTNAgentContext context)
        {
        }
    }
}

# if UNITY_EDITOR
namespace HnSF.core.AI.HTN.Nodes
{
    [Serializable]
    [UseWithContext(typeof(OperatorEffectWorldStateNode))]
    public class EffectWorldStateBlock : BlockNode
    {
        public const string optionEffectType = "EffectType";
        public const string inputStateId = "StateID";
        public const string inputStateValue = "StateValue";
        
        public override void OnEnable()
        {
            base.OnEnable();
            DefaultColor = new Color(0f, 0.5f, 0.5f, 1.0f);
        }

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
            
            context.AddOption<EffectType>(optionEffectType)
                .WithDisplayName("Effect Type")
                .WithDefaultValue(EffectType.PlanAndExecute)
                .Build();
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            base.OnDefinePorts(context);
            
            context.AddInputPort(inputStateId)
                .WithDisplayName("State ID Function")
                .Build();

            context.AddInputPort(inputStateValue)
                .WithDisplayName("State Value Function")
                .Build();
        }

        public OperatorEffectWorldState.EffectPair Convert()
        {
            GetNodeOptionByName(optionEffectType).TryGetValue<EffectType>(out var effectType);
            
            return new OperatorEffectWorldState.EffectPair()
            {
                effectType = effectType,
                stateID = NodeHelper.GetInputPortParam<HTNParamByte, byte>(GetInputPortByName(inputStateId)),
                stateValue = NodeHelper.GetInputPortParam<HTNParamByte, byte>(GetInputPortByName(inputStateValue)),
            };
        }
    }

    [Serializable]
    [UseWithGraph(typeof(PrimitiveTaskGraph))]
    public unsafe class OperatorEffectWorldStateNode : OperatorBase
    {
        public const string optionEffectType = "EffectType";
        public const string inDirtyWorldState = "DirtyWorldState";
        public const string inputStateId = "StateID";
        public const string inputStateValue = "StateValue";
        
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
        }

        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);

            context.AddInputPort<bool>(inDirtyWorldState)
                .WithDisplayName("Dirty World State?")
                .WithDefaultValue(true)
                .Build();
        }

        public override HTNOperatorBase Convert()
        {
            this.GetNodeOptionByName(OPTION_LABEL).TryGetValue<string>(out var label);
            var effectPairList = new List<Operators.OperatorEffectWorldState.EffectPair>();

            foreach (var bn in BlockNodes)
            {
                if(bn is not EffectWorldStateBlock wsb)
                    continue;
                effectPairList.Add(wsb.Convert());
            }
            
            return new Operators.OperatorEffectWorldState()
            {
                Label = label,
                dirtyWorldState = NodeHelper.GetInputPortValue<bool>(GetInputPortByName(inDirtyWorldState)),
                effectsList = effectPairList,
            };
        }
    }
}
#endif