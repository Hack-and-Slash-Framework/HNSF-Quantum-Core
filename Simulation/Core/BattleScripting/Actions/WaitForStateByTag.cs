using System;
using HnSF.core.GroupControl.Actions;
using HnSF.core.GroupControl.Functions;
using HnSF.Nodes;
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
    public unsafe partial class WaitForStateByTag : GroupControlAction
    {
        public GroupControlFunctionEntityRef[] entitiesToWaitFor = Array.Empty<GroupControlFunctionEntityRef>();
        public AssetRef<Tag>[] stateTagsToWaitFor = Array.Empty<AssetRef<Tag>>();
        
        public override void OnEnter(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
        }
        
        public override bool Tick(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            foreach (var entityFunction in entitiesToWaitFor)
            {
                var targetEntity = entityFunction.Execute(frame, infoEntityRef, ref context);
                if(targetEntity == EntityRef.None || !frame.Exists(targetEntity))
                    continue;
                if (!CheckStateTag(frame, targetEntity)) return false;
            }
            return true;
        }
        
        private bool CheckStateTag(Frame frame, EntityRef battleActorRef)
        {
            if (!frame.Unsafe.TryGetPointer<GenericStateMachine>(battleActorRef, out var gsm)
                || !frame.TryFindAsset(gsm->stateAgent.stateData.state, out var state)) return true;
            
            foreach (var tag in stateTagsToWaitFor)
            {
                if (state.sharedStateTag == tag)
                    return true;
            }
            return false;
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
    internal class WaitForStateByTagNode : ActorGroupControlNode
    {
        public const string inPortEntityRefFunctions = "EntityRefFunctions";
        public const string inPortValidTags = "TargetTag";
        
        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);

            context.AddInputPort(inPortEntityRefFunctions)
                .WithDisplayName("Entities to Wait For Functions")
                .Build();
            
            context.AddInputPort<List<AssetRef<Tag>>>(inPortValidTags)
                .WithDisplayName("State Tags to Wait For")
                .Build();
        }

        public override GroupControlAction Convert()
        {
            var targetTags = NodeHelper.GetInputPortValue<List<AssetRef<Tag>>>(GetInputPortByName(inPortValidTags));
            
            return new Actions.WaitForStateByTag()
            {
                entitiesToWaitFor = ConvertFunctionNodes<GroupControlFunctionEntityRef>(GetInputPortByName(inPortEntityRefFunctions)).ToArray(),
                stateTagsToWaitFor = targetTags.ToArray()
            };
        }
    }
}
#endif