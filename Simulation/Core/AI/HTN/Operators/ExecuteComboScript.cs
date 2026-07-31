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
    public unsafe partial class ExecuteComboScript : HTNOperatorBase
    {
        public AssetRef<HNSFState> stateToWaitFor;
        public int maxWaitTime = 1;
        
        public override HTNTaskStatus OnEnter(ref HTNAgentContext context)
        {
            if (!context.frame.Unsafe.TryGetPointer(context.agentEntityRef, out BattleActorAI* battleActorAI)
                || !context.frame.Unsafe.TryGetPointer<BattleActorLink>(battleActorAI->target, out var battleActorLink)
                || !context.frame.TryFindAsset(battleActorLink->battleActorDefinition, out var bad))
                return HTNTaskStatus.Failure;

            var gotScriptAssetRef = bad.comboScripts[context.frame.RNG->Next(0, bad.comboScripts.Length)];

            if (!context.frame.TryFindAsset(gotScriptAssetRef, out var comboScript))
                return HTNTaskStatus.Failure;

            var genericControlManager = context.frame.GetOrAddSingleton<GenericGroupControlManager>();
            var infoEntityMap = context.frame.ResolveDictionary(genericControlManager.controlInfoEntityMap);
            
            context.frame.AddOrGet(context.agentEntityRef, out TaggedEntityMapping* tagMapping);
            var tagMap = context.frame.ResolveDictionary(tagMapping->tagToEntityMap);
            if (tagMap.ContainsKey(context.frame.SimulationConfig.tag_self))
                tagMap[context.frame.SimulationConfig.tag_self] = battleActorAI->target;
            else 
                tagMap.Add(context.frame.SimulationConfig.tag_self, battleActorAI->target);
            
            context.frame.AddOrGet(context.agentEntityRef, out GenericGroupControl* ggc);
            ggc->autoDestroy = false;
            
            var basc = new BattleScriptContext();
            basc.SetScriptEntityAndBlackboard(context.frame, context.agentEntityRef, null);
            
            ggc->data.SetData(comboScript);
            ggc->data.Initialize(context.frame, context.agentEntityRef, ref basc);
            
            infoEntityMap.Add(new AssetRef((long)context.agentEntityRef.GetHashCode()), context.agentEntityRef);
            return base.OnEnter(ref context);
        }
        
        public override HTNTaskStatus Tick(ref HTNAgentContext context)
        {
            var frame = context.frame;
            
            if (!frame.Unsafe.TryGetPointer<BattleActorAI>(context.agentEntityRef, out var battleActorAI)
                || !frame.Unsafe.TryGetPointer<GenericGroupControl>(context.agentEntityRef, out var groupController))
            {
                return HTNTaskStatus.Failure;
            }
            
            /*
            var groupControlContext = new BattleScriptContext();
            groupControlContext.SetScriptEntityAndBlackboard(frame, context.agentEntityRef, null);

            if (groupController->data.IsEnd(frame, ref groupControlContext)) return HTNTaskStatus.Success;
                
            if (groupController->data.Tick(frame, context.agentEntityRef, ref groupControlContext))
            {
                if (groupController->data.IsEnd(frame, ref groupControlContext))
                {
                    groupController->data.currentAction = -1;
                    return HTNTaskStatus.Success;
                }
            }*/

            return groupController->data.currentAction == -1 ? HTNTaskStatus.Success : HTNTaskStatus.Executing;
        }
        
        public override void OnExit(ref HTNAgentContext context)
        {
            context.frame.Remove<GenericGroupControl>(context.agentEntityRef);
            
            var genericControlManager = context.frame.GetOrAddSingleton<GenericGroupControlManager>();
            var infoEntityMap = context.frame.ResolveDictionary(genericControlManager.controlInfoEntityMap);
            infoEntityMap.Remove(new AssetRef((long)context.agentEntityRef.GetHashCode()));
        }
    }
}

# if UNITY_EDITOR
namespace HnSF.core.AI.HTN.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(PrimitiveTaskGraph))]
    internal class OperatorExecuteComboScript : OperatorBase
    {
        public const string InPort_StateAssetRef = "StateAssetRef";
        public const string Option_MaxWaitTime = "MaxWaitTime";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);

            /*
            context.AddOption<int>(Option_MaxWaitTime)
                .WithDisplayName("Max Wait Time")
                .WithDefaultValue(1)
                .Build();*/
        }

        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);

            /*
            context.AddInputPort<AssetRef<HNSFState>>(InPort_StateAssetRef)
                .WithDisplayName("State")
                .Build();*/
        }

        public override HTNOperatorBase Convert()
        {
            //this.GetInputPortByName(InPort_StateAssetRef).TryGetValue(out AssetRef<HNSFState> stateAssetRef);
            //GetNodeOptionByName(Option_MaxWaitTime).TryGetValue(out int maxWaitTime);
            
            return new Operators.ExecuteComboScript()
            {
                /*
                stateToWaitFor = stateAssetRef,
                maxWaitTime = maxWaitTime*/
            };
        }
    }
}
#endif