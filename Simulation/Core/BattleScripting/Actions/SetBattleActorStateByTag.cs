using System;
using HnSF.core.GroupControl.Actions;
using HnSF.core.state;
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
    public unsafe partial class SetBattleActorStateByTag : GroupControlAction
    {
        [Serializable]
        public struct TargetAndState
        {
            public AssetRef<Tag> targetTag;
            public bool overrideMoveset;
            [DrawIf(nameof(overrideMoveset), true)]
            public AssetRef<Tag> toStateMovesetTag;
            public AssetRef<Tag> toStateTag;
            public int toFrame;
        }

        public TargetAndState[] statesToSet = Array.Empty<TargetAndState>();
        public bool immediateTransition = true;
        
        public override void OnEnter(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            foreach (var state in statesToSet)
            {
                var targetEntity = TaggedEntityMapping.GetEntityFromMap(frame, infoEntityRef, state.targetTag);
                if(targetEntity == EntityRef.None) continue;
                RequestNewState(frame, targetEntity, state);
            }
        }

        private void RequestNewState(Frame frame, EntityRef battleActorRef, TargetAndState tas)
        {
            if (frame.Unsafe.TryGetPointer<GenericStateMachine>(battleActorRef, out var gsm)
                && frame.TryFindAsset(gsm->stateAgent.stateSet, out var stateSet)
                && stateSet.AttemptGetStateByTag(tas.overrideMoveset ? tas.toStateMovesetTag : gsm->stateAgent.stateData.moveset, tas.toStateTag, out var toStateRef))
            {
                gsm->stateAgent.stateData.toStateRequested = true;
                gsm->stateAgent.stateData.toState = toStateRef;
                gsm->stateAgent.stateData.toFrame = tas.toFrame;
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
    internal class SetBattleActorStateByTagNode : ActorGroupControlNode
    {
        public const string IN_PORT_Target_Tag = "TargetTag";
        public const string IN_PORT_Override_Moveset = "OverrideMoveset";
        public const string IN_PORT_Moveset_Tag = "MovesetTag";
        public const string IN_PORT_State_Tag = "StateTag";
        public const string IN_PORT_To_Frame = "ToFrame";
        public const string IN_PORT_Immediate_Transition = "ImmediateTransition";
        
        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);
            
            context.AddInputPort<Tag>(IN_PORT_Target_Tag)
                .WithDisplayName("Target Tag")
                .Build();
            
            context.AddInputPort<bool>(IN_PORT_Override_Moveset)
                .WithDisplayName("Override Moveset")
                .Build();
            
            context.AddInputPort<Tag>(IN_PORT_Moveset_Tag)
                .WithDisplayName("Overriding Moveset")
                .Build();
            
            context.AddInputPort<Tag>(IN_PORT_State_Tag)
                .WithDisplayName("State Tag")
                .Build();
            
            context.AddInputPort<int>(IN_PORT_To_Frame)
                .WithDisplayName("To Frame")
                .Build();
            
            context.AddInputPort<bool>(IN_PORT_Immediate_Transition)
                .WithDisplayName("Immediate Transition")
                .Build();
        }

        public override GroupControlAction Convert()
        {
            var targetTag = GetInputPortValue<Tag>(this.GetInputPortByName(IN_PORT_Target_Tag));
            var overrideMoveset = GetInputPortValue<bool>(this.GetInputPortByName(IN_PORT_Override_Moveset));
            var movesetTag = GetInputPortValue<Tag>(this.GetInputPortByName(IN_PORT_Moveset_Tag));
            var stateTag = GetInputPortValue<Tag>(this.GetInputPortByName(IN_PORT_State_Tag));
            var toFrame = GetInputPortValue<int>(this.GetInputPortByName(IN_PORT_To_Frame));
            var immediateTransition = GetInputPortValue<bool>(this.GetInputPortByName(IN_PORT_Immediate_Transition));
            
            return new SetBattleActorStateByTag()
            {
                statesToSet = new []
                {
                    new SetBattleActorStateByTag.TargetAndState()
                    {
                        targetTag = targetTag,
                        overrideMoveset = overrideMoveset,
                        toStateMovesetTag = movesetTag,
                        toStateTag = stateTag,
                        toFrame = toFrame
                    }
                },
                immediateTransition = immediateTransition
            };
        }
    }
}
#endif