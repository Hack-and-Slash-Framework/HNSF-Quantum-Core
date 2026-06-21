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
#if QUANTUM_UNITY
    [MovedFrom(autoUpdateAPI: true, sourceNamespace: "HnSF.core.scripting.VersusIntro.Actions")]
#endif
    public unsafe partial class OperatorWaitForFrames : HTNOperatorBase
    {
        public int framesToWait = 60;

        public override HTNTaskStatus OnEnter(ref HTNAgentContext context)
        {
            context.frame.AddOrGet(context.agentEntityRef, out GenericTimer* gt);
            gt->countingType = TimerCountingType.CountDown;
            gt->value = framesToWait;
            return base.OnEnter(ref context);
        }

        public override HTNTaskStatus Tick(ref HTNAgentContext context)
        {
            var gt = context.frame.Unsafe.GetPointer<GenericTimer>(context.agentEntityRef);
            return gt->value <= 0 ? HTNTaskStatus.Success : HTNTaskStatus.Executing_DelayFrame;
        }

        public override void OnExit(ref HTNAgentContext context)
        {
            context.frame.Remove<GenericTimer>(context.agentEntityRef);
        }
    }
}

# if UNITY_EDITOR
namespace HnSF.core.AI.HTN.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(PrimitiveTaskGraph))]
    public unsafe class OperatorWaitForFrames : OperatorBase
    {
        public const string IN_PORT_FRAMES_TO_WAIT = "WaitFrames";
        
        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);

            context.AddInputPort<int>(IN_PORT_FRAMES_TO_WAIT)
                .WithDisplayName("Frames")
                .Build();
        }

        public override HTNOperatorBase Convert()
        {
            var frames = GetInputPortValue<int>(this.GetInputPortByName(IN_PORT_FRAMES_TO_WAIT));
            return new Operators.OperatorWaitForFrames()
            {
                framesToWait = frames
            };
        }
    }
}
#endif