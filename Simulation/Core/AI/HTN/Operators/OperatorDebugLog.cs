using System;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine.Scripting.APIUpdating;
#endif
#if UNITY_EDITOR
using HnSF.core.GroupControl.Nodes;
using Unity.GraphToolkit.Editor;
#endif

namespace HnSF.core.AI.HTN.Operators
{
    [Serializable]
    public unsafe partial class OperatorDebugLog : HTNOperatorBase
    {
        public string enterString;
        public string tickString;
        public string exitString;

        public override HTNTaskStatus OnEnter(ref HTNAgentContext context)
        {
            if(!string.IsNullOrEmpty(enterString)) Log.Debug(enterString);
            return base.OnEnter(ref context);
        }

        public override HTNTaskStatus Tick(ref HTNAgentContext context)
        {
            if(!string.IsNullOrEmpty(tickString)) Log.Debug(tickString);
            return HTNTaskStatus.Success;
        }

        public override void OnExit(ref HTNAgentContext context)
        {
            if(!string.IsNullOrEmpty(exitString)) Log.Debug(exitString);
        }
    }
}

# if UNITY_EDITOR
namespace HnSF.core.AI.HTN.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(PrimitiveTaskGraph))]
    public unsafe class Debuglog : OperatorBase
    {
        public const string IN_PORT_MESSAGE_ENTER = "MsgEnter";
        public const string IN_PORT_MESSAGE_TICK = "MsgTick";
        public const string IN_PORT_MESSAGE_EXIT = "MsgExit";
        
        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);

            context.AddInputPort<string>(IN_PORT_MESSAGE_ENTER)
                .WithDisplayName("Enter Msg")
                .Build();
            
            context.AddInputPort<string>(IN_PORT_MESSAGE_TICK)
                .WithDisplayName("Tick Msg")
                .Build();
            
            context.AddInputPort<string>(IN_PORT_MESSAGE_EXIT)
                .WithDisplayName("Exit Msg")
                .Build();
        }

        public override HTNOperatorBase Convert()
        {
            this.GetNodeOptionByName(OPTION_LABEL).TryGetValue<string>(out var label);
            var msgEnter = GetInputPortValue<string>(this.GetInputPortByName(IN_PORT_MESSAGE_ENTER));
            var msgTick = GetInputPortValue<string>(this.GetInputPortByName(IN_PORT_MESSAGE_TICK));
            var msgExit = GetInputPortValue<string>(this.GetInputPortByName(IN_PORT_MESSAGE_EXIT));
            return new Operators.OperatorDebugLog()
            {
                Label = label,
                enterString = msgEnter,
                tickString = msgTick,
                exitString = msgExit
            };
        }
    }
}
#endif