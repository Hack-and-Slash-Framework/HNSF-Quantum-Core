using System;
using HnSF.core.GroupControl;
using HnSF.core.GroupControl.Actions;
using HnSF.core.GroupControl.Combo;
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
    public unsafe partial class ExecuteCurrentCombo : HTNOperatorBase
    {
        //public AssetRef<HNSFState> stateToWaitFor;
        //public int maxWaitTime = 1;
        
        public override HTNTaskStatus OnEnter(ref HTNAgentContext context)
        {
            if (!context.frame.Unsafe.TryGetPointer(context.agentEntityRef, out BattleActorAI* battleActorAI)
                || !context.frame.Unsafe.TryGetPointer<BattleActorLink>(battleActorAI->aiActorRef, out var battleActorLink)
                || !context.frame.Unsafe.TryGetPointer<CurrentExecutingCombo>(context.agentEntityRef, out var currentExecutingComboInfo))
                return HTNTaskStatus.Failure;
            
            var nextComboIndex = currentExecutingComboInfo->comboIndex;

            if (nextComboIndex < 0)
                return HTNTaskStatus.Failure;
            
            context.frame.AddOrGet(context.agentEntityRef, out AIComboMemory* comboMemory);
            
            //if(context.frame.Number < comboMemory->nextAttackDecisionFrame)
            //    return HTNTaskStatus.Failure;
            
            context.frame.AddOrGet(context.agentEntityRef, out TaggedEntityMapping* agentTagMapping);
            var agentTagMap = context.frame.ResolveDictionary(agentTagMapping->tagToEntityMap);
            agentTagMap[context.frame.SimulationConfig.tag_self] = battleActorAI->aiActorRef;
            
            var ctx = new BattleScriptContext();
            ctx.SetScriptEntityAndBlackboard(context.frame, battleActorAI->aiActorRef, null);
            
            if (!context.frame.TryFindAsset(currentExecutingComboInfo->comboAssetRef, out var comboScript))
            {
                Log.Debug("No Combo.");
                return HTNTaskStatus.Failure;
            }

            comboMemory->nextAttackDecisionFrame = context.frame.Number + context.frame.RNG->Next(comboScript.minimumNextAttackDelayFrames, comboScript.maximumNextAttackDelayFrames);
            if (comboScript.cooldownFrames > 0)
            {
                var cooldownDict = context.frame.ResolveDictionary(comboMemory->perMoveCooldowns);
                cooldownDict[(byte)nextComboIndex] = context.frame.Number + comboScript.cooldownFrames;
            }

            var comboHistory = context.frame.ResolveList(comboMemory->history);
            if (comboHistory.Count >= 3)
            {
                for (int i = comboHistory.Count - 2; i >= 0; i--)
                {
                    comboHistory[i] = comboHistory[i + 1];
                }

                comboHistory[^1] = (byte)nextComboIndex;
            }
            else
            {
                comboHistory.Add((byte)nextComboIndex);
            }
            
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

            //context.frame.AddOrGet(context.agentEntityRef, out ExecutingBattleScriptEntityReference* bser);
            //bser->entityRef = infoEntityRef;
            currentExecutingComboInfo->scriptEntityRef = infoEntityRef;
            currentExecutingComboInfo->status = ExecutingComboStatus.Executing;
            return base.OnEnter(ref context);
        }
        
        public override HTNTaskStatus Tick(ref HTNAgentContext context)
        {
            var frame = context.frame;
            if (!frame.Unsafe.TryGetPointer<CurrentExecutingCombo>(context.agentEntityRef, out var bser))
                return HTNTaskStatus.Failure;
            
            return frame.Exists(bser->scriptEntityRef) ? HTNTaskStatus.Executing : HTNTaskStatus.Success;
        }
        
        public override void OnExit(ref HTNAgentContext context)
        {
            if (!context.frame.Unsafe.TryGetPointer<CurrentExecutingCombo>(context.agentEntityRef,
                    out var bser))
                return;
            
            bser->Cleanup(context.frame, EntityRef.None, true);
            bser->ClearCombo(context.frame);
            //context.frame.Remove<ExecutingBattleScriptEntityReference>(context.agentEntityRef);
        }
    }
}

# if UNITY_EDITOR
namespace HnSF.core.AI.HTN.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(PrimitiveTaskGraph))]
    internal class OperatorExecuteCurrentCombo : OperatorBase
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
            
            return new Operators.ExecuteCurrentCombo()
            {
                /*
                stateToWaitFor = stateAssetRef,
                maxWaitTime = maxWaitTime*/
            };
        }
    }
}
#endif