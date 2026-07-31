using System;
using HnSF.core.GroupControl;
using HnSF.core.GroupControl.Actions;
using HnSF.core.state;
using Photon.Deterministic;
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
    public unsafe partial class HTNAgentWaitForTaggedState : HTNOperatorBase
    {
        public AssetRef<Tag> stateTagToWaitFor;
        public int maxWaitTime = 1;
        
        public override HTNTaskStatus OnEnter(ref HTNAgentContext context)
        {
            context.frame.AddOrGet(context.agentEntityRef, out GenericTimer* gt);
            gt->countingType = TimerCountingType.CountUp;
            gt->value = 0;
            return base.OnEnter(ref context);
        }
        
        public override HTNTaskStatus Tick(ref HTNAgentContext context)
        {
            var frame = context.frame;
            
            if (!frame.Unsafe.TryGetPointer<BattleActorAI>(context.agentEntityRef, out var battleActorAI)
                || !frame.Unsafe.TryGetPointer<GenericStateMachine>(battleActorAI->target, out var gsm)
                || !frame.TryFindAsset(gsm->stateAgent.stateSet, out var stateSet)
                || !stateSet.AttemptGetStateByTag(gsm->stateAgent.stateData.moveset, stateTagToWaitFor, out var stateRef))
            {
                //SetAgentActionResult(frame, HTNTaskResult.FAILURE, ref context);
                return HTNTaskStatus.Executing;
            }
            
            // Time up.
            var gt = frame.Unsafe.GetPointer<GenericTimer>(context.agentEntityRef);
            if (gt->value > maxWaitTime)
            {
                //SetAgentActionResult(frame, HTNTaskResult.FAILURE, ref context);
                return HTNTaskStatus.Executing;
            }

            if (gsm->stateAgent.stateData.state == stateRef)
            {
                return HTNTaskStatus.Success;
            }

            return HTNTaskStatus.Failure;
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
    internal class OperatorWaitForTaggedStateNode : OperatorBase
    {
        public const string InPort_StateAssetRef = "StateTagAssetRef";
        public const string Option_MaxWaitTime = "MaxWaitTime";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);

            context.AddOption<int>(Option_MaxWaitTime)
                .WithDisplayName("Max Wait Time")
                .WithDefaultValue(1)
                .Build();
        }

        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);

            context.AddInputPort<AssetRef<Tag>>(InPort_StateAssetRef)
                .WithDisplayName("State Tag")
                .Build();
        }

        public override HTNOperatorBase Convert()
        {
            this.GetInputPortByName(InPort_StateAssetRef).TryGetValue(out AssetRef<Tag> stateAssetRef);
            GetNodeOptionByName(Option_MaxWaitTime).TryGetValue(out int maxWaitTime);
            
            return new Operators.HTNAgentWaitForTaggedState()
            {
                stateTagToWaitFor = stateAssetRef,
                maxWaitTime = maxWaitTime
            };
        }
    }
}
#endif