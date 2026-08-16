using System;
using System.Collections.Generic;
using HnSF.core.GroupControl.Actions;
using Quantum;
#if QUANTUM_UNITY
#endif
#if UNITY_EDITOR
using HnSF.Nodes;
using Unity.GraphToolkit.Editor;
#endif

namespace HnSF.core.GroupControl.Actions
{
    [Serializable]
    public unsafe partial class PlaySyncedCutscene : GroupControlAction
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
        
        public override void OnEnter(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            foreach (var state in statesToSet)
            {
                PlaySyncedCutsceneFor(frame, infoEntityRef, state);
            }
        }

        private void PlaySyncedCutsceneFor(Frame frame, EntityRef playingEntityRef, TargetAndState tas)
        {
            //var sccEntity = frame.Create();
            var scc = new SyncedCutsceneSource()
            {
                sourcePlayer = playingEntityRef,
                cutsceneSource = tas.cutsceneSource,
                cutsceneTag = tas.cutsceneTag,
                frame = 0,
                playrate = 1,
                ignorePlayerLdt = tas.ignoreLdt,
                autoPlay = tas.autoPlay,
                autoEnd = tas.autoEnd,
                endFrame = tas.autoEndFrame
            };
            frame.Add(playingEntityRef, scc, out var sccResult);
            
            if (tas.localOnly)
            {
                var specificPlayers = frame.ResolveList(sccResult->onlyFor);

                if (frame.Unsafe.TryGetPointer<PlayerLink>(playingEntityRef, out var playerLink))
                {
                    specificPlayers.Add(playerLink->Player);
                }
                else
                {
                    //frame.Destroy(sccEntity);
                    frame.Remove<SyncedCutsceneSource>(playingEntityRef);
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
    internal class PlaySyncedCutscene: ActorGroupControlNode
    {
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

            context.AddInputPort(IN_PORT_Controlled_Entities)
                .WithDisplayName("Controlled Entities")
                .WithConnectorUI(PortConnectorUI.Circle)
                .Build();
        }

        public override GroupControlAction Convert()
        {
            this.GetNodeOptionByName(OPTION_LABEL).TryGetValue<string>(out var label);
            var cutsceneSource = NodeHelper.GetInputPortValue<AssetObject>(this.GetInputPortByName(IN_PORT_Cutscene_Source));
            var cutsceneTag = NodeHelper.GetInputPortValue<Tag>(this.GetInputPortByName(IN_PORT_Cutscene_Tag));
            var autoplay = NodeHelper.GetInputPortValue<bool>(this.GetInputPortByName(IN_PORT_Autoplay));
            var autoend = NodeHelper.GetInputPortValue<bool>(this.GetInputPortByName(IN_PORT_Autoend));
            var ignoreLdt = NodeHelper.GetInputPortValue<bool>(this.GetInputPortByName(IN_PORT_IgnoreLdt));
            var autoendFrame = NodeHelper.GetInputPortValue<int>(this.GetInputPortByName(IN_PORT_Autoend_Frame));
            var localOnly = NodeHelper.GetInputPortValue<bool>(this.GetInputPortByName(IN_PORT_Local_Only));

            var cutsceneControlledEntities = new List<HnSF.core.GroupControl.Actions.PlaySyncedCutscene.TagToTag>();
            var controlledEntitiesNodePorts = new List<IPort>();
            this.GetInputPortByName(IN_PORT_Controlled_Entities).GetConnectedPorts(controlledEntitiesNodePorts);

            foreach (var np in controlledEntitiesNodePorts)
            {
                var node = np.GetNode();
                if(!(node is ControlledEntityInfo cei)) continue;

                cei.GetNodeOptionByName(ControlledEntityInfo.CONSTANT_Entity_Tag).TryGetValue<AssetRef<Tag>>(out var entityTag);
                cei.GetNodeOptionByName(ControlledEntityInfo.CONSTANT_ControlPositon).TryGetValue<bool>(out var controlPositon);
                cei.GetNodeOptionByName(ControlledEntityInfo.CONSTANT_ControlAnimation).TryGetValue<bool>(out var controlAnimation);
                
                cutsceneControlledEntities.Add(new Actions.PlaySyncedCutscene.TagToTag()
                {
                    entityTag = entityTag,
                    dontControlAnimation = !controlAnimation,
                    dontControlPosition = !controlPositon
                });
            }
            
            return new HnSF.core.GroupControl.Actions.PlaySyncedCutscene()
            {
                Label = label,
                statesToSet = new []
                {
                    new HnSF.core.GroupControl.Actions.PlaySyncedCutscene.TargetAndState()
                    {
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

    [Serializable]
    [UseWithGraph(typeof(ActorGroupScriptGraph))]
    internal class ControlledEntityInfo : ControlNodeBase
    {
        public const string OUT_PORT_DEFAULT = "Output";
        public const string CONSTANT_Entity_Tag = "EntityTag";
        public const string CONSTANT_ControlPositon = "ControlPosition";
        public const string CONSTANT_ControlAnimation = "ControlAnimation";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);

            context.AddOption<AssetRef<Tag>>(CONSTANT_Entity_Tag)
                .WithDisplayName("Entity Tag")
                .Build();
            
            context.AddOption<bool>(CONSTANT_ControlPositon)
                .WithDisplayName("Control Position")
                .Build();
            
            context.AddOption<bool>(CONSTANT_ControlAnimation)
                .WithDisplayName("Control Animation")
                .Build();
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            base.OnDefinePorts(context);
            
            context.AddOutputPort(OUT_PORT_DEFAULT)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }
    }
}
#endif