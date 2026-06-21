using System;
using HnSF.core.GroupControl.Actions;
using Quantum;
#if QUANTUM_UNITY
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
#if QUANTUM_UNITY
    [MovedFrom(autoUpdateAPI: true, sourceNamespace: "HnSF.core.scripting.VersusIntro.Actions")]
#endif
    public unsafe partial class WaitForEndOfState : GroupControlAction
    {
        [Serializable]
        public struct TargetAndState
        {
            public AssetRef<Tag> targetTag;
        }

        public TargetAndState[] statesToSet = Array.Empty<TargetAndState>();
        
        public override void OnEnter(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
        }
        
        public override bool Tick(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            foreach (var state in statesToSet)
            {
                var targetEntity = TaggedEntityMapping.GetEntityFromMap(frame, infoEntityRef, state.targetTag);
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
        public const string inPortValidTags = "TargetTag";
        
        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);

            context.AddInputPort<List<AssetRef<Tag>>>(inPortValidTags)
                .WithDisplayName("Valid State Tags")
                .Build();
        }

        public override GroupControlAction Convert()
        {
            var targetTags = GetInputPortValue<List<AssetRef<Tag>>>(GetInputPortByName(inPortValidTags));

            var statesToSet = targetTags == null ? Array.Empty<WaitForEndOfState.TargetAndState>() : new WaitForEndOfState.TargetAndState[targetTags.Count];

            for (int i = 0; i < targetTags.Count; i++)
            {
                statesToSet[i] = new WaitForEndOfState.TargetAndState()
                {
                    targetTag = targetTags[i],
                };
            }
            
            return new WaitForEndOfState()
            {
                statesToSet = statesToSet
            };
        }
    }
}
#endif