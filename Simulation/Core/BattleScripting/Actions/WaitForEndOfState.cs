using System;
using HnSF.core.GroupControl.Actions;
using HnSF.core.GroupControl.Functions;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
#endif
#if UNITY_EDITOR
using System.Collections.Generic;
using HnSF.core.GroupControl.Nodes;
using Unity.GraphToolkit.Editor;
#endif

namespace HnSF.core.GroupControl.Actions
{
    [Serializable]
    public unsafe partial class WaitForEndOfState : GroupControlAction
    {
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public GroupControlFunctionEntityRef[] entitiesToWaitFor = Array.Empty<GroupControlFunctionEntityRef>();
        
        public override void OnEnter(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
        }
        
        public override bool Tick(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            foreach (var entityFunction in entitiesToWaitFor)
            {
                var targetEntity = entityFunction.Execute(frame, infoEntityRef, ref context);
                if(targetEntity == EntityRef.None) continue;
                if (!CheckStateOver(frame, targetEntity)) return false;
            }
            return true;
        }
        
        private bool CheckStateOver(Frame frame, EntityRef battleActorRef)
        {
            if (frame.Unsafe.TryGetPointer<GenericStateMachine>(battleActorRef, out var gsm)
                && frame.TryFindAsset(gsm->stateAgent.stateData.state, out var state))
            {
                return gsm->stateAgent.stateData.frame >= state.totalFrames;
            }
            return true;
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
    internal class WaitForEndOfStateNode : ActorGroupControlNode
    {
        public const string inPortEntityFunctions = "TargetTag";
        
        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);

            context.AddInputPort(inPortEntityFunctions)
                .WithDisplayName("Entities to Wait For Functions")
                .Build();
        }

        public override GroupControlAction Convert()
        {
            return new Actions.WaitForEndOfState()
            {
                entitiesToWaitFor = ConvertFunctionNodes<GroupControlFunctionEntityRef>(GetInputPortByName(inPortEntityFunctions)).ToArray(),
            };
        }
    }
}
#endif