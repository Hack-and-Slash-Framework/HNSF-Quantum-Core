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
                || !context.frame.Unsafe.TryGetPointer<BattleActorLink>(battleActorAI->aiActorRef, out var battleActorLink)
                || !context.frame.TryFindAsset(battleActorLink->battleActorDefinition, out var bad))
                return HTNTaskStatus.Failure;

            var gotScriptAssetRef = bad.comboScripts[context.frame.RNG->Next(0, bad.comboScripts.Length)];

            if (!context.frame.TryFindAsset(gotScriptAssetRef, out var comboScript))
                return HTNTaskStatus.Failure;

            var genericControlManager = context.frame.GetOrAddSingleton<GenericGroupControlManager>();

            var infoEntityRef = context.frame.Create();
            
            context.frame.AddOrGet(infoEntityRef, out TaggedEntityMapping* tagMapping);
            var tagMap = context.frame.ResolveDictionary(tagMapping->tagToEntityMap);
            tagMap[context.frame.SimulationConfig.tag_self] = battleActorAI->aiActorRef;
            
            context.frame.AddOrGet(infoEntityRef, out GenericGroupControl* ggc);
            ggc->autoDestroy = true;
            
            var basc = new BattleScriptContext();
            basc.SetScriptEntityAndBlackboard(context.frame, infoEntityRef, null);
            
            ggc->data.SetData(comboScript);
            ggc->data.Initialize(context.frame, infoEntityRef, ref basc);

            genericControlManager.Add(context.frame, EntityRef.None, infoEntityRef);

            context.frame.AddOrGet(context.agentEntityRef, out ExecutingBattleScriptEntityReference* bser);
            bser->entityRef = infoEntityRef;
            return base.OnEnter(ref context);
        }
        
        public override HTNTaskStatus Tick(ref HTNAgentContext context)
        {
            var frame = context.frame;
            if (!frame.Unsafe.TryGetPointer<ExecutingBattleScriptEntityReference>(context.agentEntityRef, out var bser))
                return HTNTaskStatus.Failure;
            
            return frame.Exists(bser->entityRef) ? HTNTaskStatus.Executing : HTNTaskStatus.Success;
        }
        
        public override void OnExit(ref HTNAgentContext context)
        {
            if (!context.frame.Unsafe.TryGetPointer<ExecutingBattleScriptEntityReference>(context.agentEntityRef,
                    out var bser))
                return;
            
            bser->Cleanup(context.frame, EntityRef.None, true);
            context.frame.Remove<ExecutingBattleScriptEntityReference>(context.agentEntityRef);
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