using System;
using HnSF.core.GroupControl.Actions;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine.Scripting.APIUpdating;
#endif
#if UNITY_EDITOR
using HnSF.Nodes;
using Unity.GraphToolkit.Editor;
#endif

namespace HnSF.core.GroupControl.Actions
{
    [Serializable]
#if QUANTUM_UNITY
    [MovedFrom(autoUpdateAPI: true, sourceNamespace: "HnSF.core.scripting.VersusIntro.Actions")]
#endif
    public unsafe partial class WaitForFrames : GroupControlAction
    {
        public int framesToWait = 60;
        
        public override void OnEnter(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            frame.AddOrGet(infoEntityRef, out GenericTimer* gt);
            gt->countingType = TimerCountingType.CountDown;
            gt->value = framesToWait;
        }

        public override bool Tick(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            var gt = frame.Unsafe.GetPointer<GenericTimer>(infoEntityRef);
            return gt->value <= 0;
        }

        public override void OnExit(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            frame.Remove<GenericTimer>(infoEntityRef);
        }
    }
}

# if UNITY_EDITOR
namespace HnSF.core.GroupControl.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(ActorGroupScriptGraph))]
    internal class WaitForFramesNode : ActorGroupControlNode
    {
        public const string IN_PORT_FRAMES_TO_WAIT = "WaitFrames";
        
        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);

            context.AddInputPort<int>(IN_PORT_FRAMES_TO_WAIT)
                .WithDisplayName("Frames")
                .Build();
        }

        public override GroupControlAction Convert()
        {
            var frames = NodeHelper.GetInputPortValue<int>(this.GetInputPortByName(IN_PORT_FRAMES_TO_WAIT));
            return new WaitForFrames()
            {
                framesToWait = frames
            };
        }
    }
}
#endif