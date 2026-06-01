using System;
using HnSF.core.GroupControl;
using HnSF.core.GroupControl.Actions;
using Quantum;
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
    public unsafe partial class HTNAgentSetTaskResult : HTNAgentAction
    {
        public HTNTaskResult taskResult;
        
        public override void OnEnter(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            base.OnEnter(frame, infoEntityRef, ref context);
            
            var contextData = (HTNAgentContext*)context.CustomData;
            if (contextData == null) return;
            contextData->agent->currentActionResult = taskResult;
        }
    }
}

# if UNITY_EDITOR
namespace HnSF.core.AI.HTN.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(ActorGroupScriptGraph))]
    internal class HTNAgentSetTaskResultNode : ActorGroupControlNode
    {
        public const string OPTION_TASK_RESULT = "Task Result";
        
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
            
            context.AddOption<HTNTaskResult>(OPTION_TASK_RESULT)
                .WithDisplayName("Task Result")
                .Build();
        }

        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);
        }

        public override GroupControlAction Convert()
        {
            GetNodeOptionByName(OPTION_TASK_RESULT).TryGetValue(out HTNTaskResult taskResult);
            
            return new Actions.HTNAgentSetTaskResult()
            {
                taskResult = taskResult
            };
        }
    }
}
#endif