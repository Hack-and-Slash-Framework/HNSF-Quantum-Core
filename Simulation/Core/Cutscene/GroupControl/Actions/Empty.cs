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
    public unsafe partial class Empty : GroupControlAction
    {
        
        public override void OnEnter(Frame frame, EntityRef infoEntityRef)
        {
        }

        public override bool Tick(Frame frame, EntityRef infoEntityRef)
        {
            return true;
        }

        public override void OnExit(Frame frame, EntityRef infoEntityRef)
        {
        }
    }
}

# if UNITY_EDITOR
namespace HnSF.core.GroupControl.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(ActorGroupScriptGraph))]
    internal class EmptyNode : ActorGroupControlNode
    {
        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);
        }

        public override GroupControlAction Convert()
        {
            this.GetNodeOptionByName(OPTION_LABEL).TryGetValue<string>(out var label);
            return new Empty()
            {
                Label = label
            };
        }
    }
}
#endif