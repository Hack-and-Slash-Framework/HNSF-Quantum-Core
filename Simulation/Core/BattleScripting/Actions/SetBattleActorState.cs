using System;
using HnSF.core.GroupControl.Actions;
using HnSF.core.state;
using HnSF.Nodes;
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
    public unsafe partial class SetBattleActorState : GroupControlAction
    {
        [Serializable]
        public struct TargetAndState
        {
            public AssetRef<Tag> targetTag;
            public AssetRef<HNSFState> state;
        }

        public TargetAndState[] statesToSet = Array.Empty<TargetAndState>();
        public bool immediateTransition = true;
        
        public override void OnEnter(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            foreach (var state in statesToSet)
            {
                var targetEntity = TaggedEntityMapping.GetEntityFromMap(frame, infoEntityRef, state.targetTag);
                if(targetEntity == EntityRef.None) continue;
                RequestNewState(frame, targetEntity, state.state);
            }
        }

        private void RequestNewState(Frame frame, EntityRef battleActorRef, AssetRef<HNSFState> stateAssetRef)
        {
            if (frame.Unsafe.TryGetPointer<GenericStateMachine>(battleActorRef, out var gsm))
            {
                gsm->stateAgent.stateData.toStateRequested = true;
                gsm->stateAgent.stateData.toState = stateAssetRef;
                gsm->stateAgent.stateData.toFrame = 0;
            }
            
            if(immediateTransition) HNSFStateHelper.Generic.CheckForStateChange(frame, battleActorRef);
        }

        public override bool Tick(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
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
    internal class SetBattleActorStateNode : ActorGroupControlNode
    {
        public const string IN_PORT_Target_Tag = "TargetTag";
        public const string IN_PORT_State = "State";
        
        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);
            
            context.AddInputPort<Tag>(IN_PORT_Target_Tag)
                .WithDisplayName("Target Tag")
                .Build();
            
            context.AddInputPort<HNSFState>(IN_PORT_State)
                .WithDisplayName("State")
                .Build();
        }

        public override GroupControlAction Convert()
        {
            var targetTag = NodeHelper.GetInputPortValue<Tag>(this.GetInputPortByName(IN_PORT_Target_Tag));
            var state = NodeHelper.GetInputPortValue<HNSFState>(this.GetInputPortByName(IN_PORT_State));
            return new SetBattleActorState()
            {
                statesToSet = new []
                {
                    new SetBattleActorState.TargetAndState()
                    {
                        targetTag = targetTag,
                        state = state
                    }
                }
            };
        }
    }
}
#endif