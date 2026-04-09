using System;
using HnSF.core.GroupControl.Actions;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine.Scripting.APIUpdating;
#endif
#if UNITY_EDITOR
using HnSF.core.GroupControl.Nodes;
using Unity.GraphToolkit.Editor;
#endif

namespace HnSF.core.GroupControl.Actions
{
    [Serializable]
#if QUANTUM_UNITY
    [MovedFrom(autoUpdateAPI: true, sourceNamespace: "HnSF.core.scripting.VersusIntro.Actions")]
#endif
    public unsafe partial class DebugLog : GroupControlAction
    {
        public string enterString;
        public string tickString;
        public string exitString;
        
        public override void OnEnter(Frame frame, EntityRef infoEntityRef)
        {
            if(!string.IsNullOrEmpty(enterString)) Log.Debug(enterString);
        }

        public override bool Tick(Frame frame, EntityRef infoEntityRef)
        {
            if(!string.IsNullOrEmpty(tickString)) Log.Debug(tickString);
            return true;
        }

        public override void OnExit(Frame frame, EntityRef infoEntityRef)
        {
            if(!string.IsNullOrEmpty(exitString)) Log.Debug(exitString);
        }
    }
}

# if UNITY_EDITOR
namespace HnSF.core.GroupControl.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(ActorGroupScriptGraph))]
    internal class DebugLogNode : ActorGroupControlNode
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

        public override GroupControlAction Convert()
        {
            this.GetNodeOptionByName(OPTION_LABEL).TryGetValue<string>(out var label);
            var msgEnter = ActorGroupScriptDirectorImporter.GetInputPortValue<string>(this.GetInputPortByName(IN_PORT_MESSAGE_ENTER));
            var msgTick = ActorGroupScriptDirectorImporter.GetInputPortValue<string>(this.GetInputPortByName(IN_PORT_MESSAGE_TICK));
            var msgExit = ActorGroupScriptDirectorImporter.GetInputPortValue<string>(this.GetInputPortByName(IN_PORT_MESSAGE_EXIT));
            return new DebugLog()
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