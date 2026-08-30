using System;
using System.Collections.Generic;
using HnSF.core.GroupControl.Actions;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine.Scripting.APIUpdating;
#endif
#if UNITY_EDITOR
using HnSF.Nodes;
using Unity.GraphToolkit.Editor;
#endif

namespace HnSF.core.GroupControl.Actions
{
    [Serializable]
#if QUANTUM_UNITY
    [MovedFrom(autoUpdateAPI: true, sourceNamespace: "HnSF.core.scripting.VersusIntro.Actions")]
#endif
    public unsafe partial class PlaySyncedCutsceneForBattleActor : GroupControlAction
    {
        [Serializable]
        public struct TagToTag
        {
            public AssetRef<Tag> entityTag;
            public bool dontControlPosition;
            public bool dontControlAnimation;
        }
        
        [Serializable]
        public struct TargetAndState
        {
            public AssetRef<Tag> targetTag;
            public AssetRef cutsceneSource;
            public AssetRef<Tag> cutsceneTag;

            public bool ignoreLdt;
            public bool autoPlay;
            public bool autoEnd;
            public int autoEndFrame;
            public bool localOnly;

            public TagToTag[] cutsceneControlledEntities;
        }

        public TargetAndState[] statesToSet = Array.Empty<TargetAndState>();
        
        public override BattleScriptResult OnEnter(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            foreach (var state in statesToSet)
            {
                EntityRef targetEntity = infoEntityRef;
                if (state.targetTag != default)
                {
                    targetEntity = TaggedEntityMapping.GetEntityFromMap(frame, infoEntityRef, state.targetTag);
                }
                if(targetEntity == EntityRef.None || !frame.Exists(targetEntity)) continue;
                
                PlaySyncedCutsceneFor(frame, targetEntity, state);
            }

            return BattleScriptResult.Succeeded;
        }

        private void PlaySyncedCutsceneFor(Frame frame, EntityRef battleActorRef, TargetAndState tas)
        {
            var sccEntity = frame.Create();
            var scc = new SyncedCutsceneSource()
            {
                sourcePlayer = battleActorRef,
                cutsceneSource = tas.cutsceneSource,
                cutsceneTag = tas.cutsceneTag,
                frame = 0,
                playrate = 1,
                ignorePlayerLdt = tas.ignoreLdt,
                autoPlay = tas.autoPlay,
                autoEnd = tas.autoEnd,
                endFrame = tas.autoEndFrame
            };
            frame.Add(sccEntity, scc, out var sccResult);
            
            if (tas.localOnly)
            {
                var specificPlayers = frame.ResolveList(sccResult->onlyFor);

                if (frame.Unsafe.TryGetPointer<PlayerLink>(battleActorRef, out var playerLink))
                {
                    specificPlayers.Add(playerLink->Player);
                }
                else
                {
                    frame.Destroy(sccEntity);
                    return;
                }
            }
            
            var d = frame.ResolveDictionary(sccResult->cutsceneControls);
            if (tas.cutsceneControlledEntities == null || (tas.cutsceneControlledEntities.Length == 0)) return;
            foreach (var cce in tas.cutsceneControlledEntities)
            {
                d[cce.entityTag] = new CutsceneEntityControlDefinition()
                {
                    controlPosition = !cce.dontControlPosition,
                    controlAnimation = !cce.dontControlAnimation,
                };
            }
        }
    }
}

# if UNITY_EDITOR
namespace HnSF.core.GroupControl.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(ActorGroupScriptGraph))]
    internal class PlaySyncedCutsceneForBattleActorNode : ActorGroupControlNode
    {
        public const string IN_PORT_Target_Tag = "TargetTag";
        public const string IN_PORT_Cutscene_Source = "CutsceneSource";
        public const string IN_PORT_Cutscene_Tag = "CutsceneTag";
        public const string IN_PORT_Autoplay = "Autoplay";
        public const string IN_PORT_IgnoreLdt = "IgnoreLdt";
        public const string IN_PORT_Autoend = "Autoend";
        public const string IN_PORT_Autoend_Frame = "AutoendFrame";
        public const string IN_PORT_Local_Only = "LocalOnly";
        public const string IN_PORT_Controlled_Entities = "ControlledEntities";
        
        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);
            
            context.AddInputPort<Tag>(IN_PORT_Target_Tag)
                .WithDisplayName("Target")
                .Build();

            context.AddInputPort<AssetObject>(IN_PORT_Cutscene_Source)
                .WithDisplayName("Cutscene Source")
                .Build();
            
            context.AddInputPort<Tag>(IN_PORT_Cutscene_Tag)
                .WithDisplayName("Cutscene")
                .Build();
            
            context.AddInputPort<bool>(IN_PORT_Autoplay)
                .WithDisplayName("Autoplay")
                .Build();
            
            context.AddInputPort<bool>(IN_PORT_IgnoreLdt)
                .WithDisplayName("Ignore Entity Local Delta Time")
                .Build();
            
            context.AddInputPort<bool>(IN_PORT_Autoend)
                .WithDisplayName("Autoend")
                .Build();
            
            context.AddInputPort<int>(IN_PORT_Autoend_Frame)
                .WithDisplayName("Autoend Frame")
                .Build();
            
            context.AddInputPort<bool>(IN_PORT_Local_Only)
                .WithDisplayName("Local Only")
                .Build();
        }

        public override GroupControlAction Convert()
        {
            this.GetNodeOptionByName(OPTION_LABEL).TryGetValue<string>(out var label);
            var targetTag = NodeHelper.GetInputPortValue<Tag>(this.GetInputPortByName(IN_PORT_Target_Tag));
            var cutsceneSource = NodeHelper.GetInputPortValue<AssetObject>(this.GetInputPortByName(IN_PORT_Cutscene_Source));
            var cutsceneTag = NodeHelper.GetInputPortValue<Tag>(this.GetInputPortByName(IN_PORT_Cutscene_Tag));
            var autoplay = NodeHelper.GetInputPortValue<bool>(this.GetInputPortByName(IN_PORT_Autoplay));
            var autoend = NodeHelper.GetInputPortValue<bool>(this.GetInputPortByName(IN_PORT_Autoend));
            var ignoreLdt = NodeHelper.GetInputPortValue<bool>(this.GetInputPortByName(IN_PORT_IgnoreLdt));
            var autoendFrame = NodeHelper.GetInputPortValue<int>(this.GetInputPortByName(IN_PORT_Autoend_Frame));
            var localOnly = NodeHelper.GetInputPortValue<bool>(this.GetInputPortByName(IN_PORT_Local_Only));
            
            var cutsceneControlledEntities = new List<HnSF.core.GroupControl.Actions.PlaySyncedCutsceneForBattleActor.TagToTag>();
            var controlledEntitiesNodePorts = new List<IPort>();
            this.GetInputPortByName(IN_PORT_Controlled_Entities).GetConnectedPorts(controlledEntitiesNodePorts);

            foreach (var np in controlledEntitiesNodePorts)
            {
                var node = np.GetNode();
                if(!(node is ControlledEntityInfo cei)) continue;

                cei.GetNodeOptionByName(ControlledEntityInfo.CONSTANT_Entity_Tag).TryGetValue<AssetRef<Tag>>(out var entityTag);
                cei.GetNodeOptionByName(ControlledEntityInfo.CONSTANT_ControlPositon).TryGetValue<bool>(out var controlPositon);
                cei.GetNodeOptionByName(ControlledEntityInfo.CONSTANT_ControlAnimation).TryGetValue<bool>(out var controlAnimation);
                
                cutsceneControlledEntities.Add(new Actions.PlaySyncedCutsceneForBattleActor.TagToTag()
                {
                    entityTag = entityTag,
                    dontControlAnimation = !controlAnimation,
                    dontControlPosition = !controlPositon
                });
            }
            
            return new PlaySyncedCutsceneForBattleActor()
            {
                Label = label,
                statesToSet = new []
                {
                    new PlaySyncedCutsceneForBattleActor.TargetAndState()
                    {
                        targetTag = targetTag,
                        cutsceneSource = cutsceneSource,
                        cutsceneTag = cutsceneTag,
                        autoPlay = autoplay,
                        autoEnd = autoend,
                        ignoreLdt = ignoreLdt,
                        autoEndFrame = autoendFrame,
                        cutsceneControlledEntities = cutsceneControlledEntities.ToArray(),
                        localOnly = localOnly
                    }
                }
            };
        }
    }
}
#endif