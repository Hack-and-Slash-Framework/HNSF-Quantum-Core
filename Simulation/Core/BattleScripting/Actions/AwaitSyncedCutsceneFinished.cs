using System;
using System.Collections.Generic;
using HnSF.core.GroupControl.Actions;
using HnSF.core.GroupControl.Functions;
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
    public unsafe partial class AwaitSyncedCutsceneFinished : GroupControlAction
    {
        public GroupControlFunctionEntityRef entityRefFunction;
        public int waitForFrame = 0;
        public int timeout;
        
        public override void OnEnter(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
        }
        
        public override bool Tick(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            var syncedSourceEntityRef = entityRefFunction == null ? infoEntityRef : entityRefFunction.Execute(frame, infoEntityRef, ref context);
            if (syncedSourceEntityRef == default || !frame.Exists(syncedSourceEntityRef))
                return true;
            if (!frame.Unsafe.TryGetPointer<SyncedCutsceneSource>(syncedSourceEntityRef, out var scs))
                return true;

            if (waitForFrame > 0)
                return scs->frame >= waitForFrame;
            
            return scs->frame >= scs->endFrame;
        }

        public override void OnExit(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
        }
    }
}

# if UNITY_EDITOR
namespace HnSF.core.GroupControl.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(ActorGroupScriptGraph))]
    internal class AwaitSyncedCutsceneFinished : ActorGroupControlNode
    {
        public const string IN_PORT_EntityFunction = "EntityFunction";
        public const string IN_PORT_FrameToWaitFor = "FrameToWaitFor";
        public const string IN_PORT_Timeout = "Timeout";
        
        
        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);
            
            context.AddInputPort(IN_PORT_EntityFunction)
                .WithDisplayName("Entity Function")
                .Build();
            
            context.AddInputPort<int>(IN_PORT_FrameToWaitFor)
                .WithDisplayName("Frame to Finish At")
                .Build();
            
            context.AddInputPort<int>(IN_PORT_Timeout)
                .WithDisplayName("Timeout")
                .Build();
        }

        public override GroupControlAction Convert()
        {
            this.GetNodeOptionByName(OPTION_LABEL).TryGetValue<string>(out var label);
            
            return new HnSF.core.GroupControl.Actions.AwaitSyncedCutsceneFinished()
            {
                Label = label,
                entityRefFunction = ConvertFunctionNode<GroupControlFunctionEntityRef>(GetInputPortByName(IN_PORT_EntityFunction)),
                waitForFrame = GetInputPortValue<int>(GetInputPortByName(IN_PORT_FrameToWaitFor)),
                timeout = GetInputPortValue<int>(GetInputPortByName(IN_PORT_Timeout)),
            };
        }
    }
}
#endif