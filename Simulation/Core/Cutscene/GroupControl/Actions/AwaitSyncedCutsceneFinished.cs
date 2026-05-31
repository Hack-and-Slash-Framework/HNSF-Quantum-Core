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
        
        public override void OnEnter(Frame frame, EntityRef infoEntityRef, ref GroupControlContext context)
        {
        }
        
        public override bool Tick(Frame frame, EntityRef infoEntityRef, ref GroupControlContext context)
        {
            var syncedSourceEntityRef = entityRefFunction == null ? infoEntityRef : entityRefFunction.Execute(frame, infoEntityRef);
            if (syncedSourceEntityRef == default || !frame.Exists(syncedSourceEntityRef))
                return true;
            if (!frame.Unsafe.TryGetPointer<SyncedCutsceneSource>(syncedSourceEntityRef, out var scs))
                return true;

            if (waitForFrame > 0)
                return scs->frame >= waitForFrame;
            
            return scs->frame >= scs->endFrame;
        }

        public override void OnExit(Frame frame, EntityRef infoEntityRef, ref GroupControlContext context)
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
            
            this.GetInputPortByName(IN_PORT_FrameToWaitFor).TryGetValue(out int frameToWaitFor);
            this.GetInputPortByName(IN_PORT_Timeout).TryGetValue(out int timeoutFrame);
            var portEntityRef = this.GetInputPortByName(IN_PORT_EntityFunction).firstConnectedPort;

            GroupControlFunctionEntityRef entityRefFunction = null;
            
            if (portEntityRef?.GetNode() is FunctionNodeBase fnEntityRef)
            {
                entityRefFunction = fnEntityRef.Convert() as GroupControlFunctionEntityRef;
            }
            
            return new HnSF.core.GroupControl.Actions.AwaitSyncedCutsceneFinished()
            {
                Label = label,
                entityRefFunction =  entityRefFunction,
                waitForFrame = frameToWaitFor,
                timeout = timeoutFrame,
            };
        }
    }
}
#endif