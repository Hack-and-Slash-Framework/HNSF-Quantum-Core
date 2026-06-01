using System;
using HnSF.core.GroupControl;
using HnSF.core.GroupControl.Actions;
using Photon.Deterministic;
using Quantum;
using Quantum.Physics2D;
#if QUANTUM_UNITY
using UnityEngine.Scripting.APIUpdating;
#endif
#if UNITY_EDITOR
using HnSF.core.GroupControl.Nodes;
using Unity.GraphToolkit.Editor;
#endif

namespace HnSF.core.AI.HTN.Actions
{
    [Serializable]
    public unsafe partial class HTNAgentSetUninterruptibleStatus : HTNAgentAction
    {
        public bool uninterruptible;
        
        public override void OnEnter(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            base.OnEnter(frame, infoEntityRef, ref context);
            
            var contextData = (HTNAgentContext*)context.CustomData;
            if (contextData == null) return;
            contextData->agent->uninterruptible = uninterruptible;
        }
    }
}

# if UNITY_EDITOR
namespace HnSF.core.AI.HTN.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(ActorGroupScriptGraph))]
    internal class HTNAgentSetUninterruptibleStatus : ActorGroupControlNode
    {
        public const string OPTION_UNINTERRUPTIBLE = "Uninterruptible";
        
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
            
            context.AddOption<bool>(OPTION_UNINTERRUPTIBLE)
                .WithDisplayName("Uninterruptible")
                .Build();
        }

        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);
        }

        public override GroupControlAction Convert()
        {
            GetNodeOptionByName(OPTION_UNINTERRUPTIBLE).TryGetValue(out bool uninterruptible);
            
            return new Actions.HTNAgentSetUninterruptibleStatus()
            {
                uninterruptible = uninterruptible
            };
        }
    }
}
#endif