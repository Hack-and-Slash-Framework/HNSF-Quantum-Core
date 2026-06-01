using System;
using HnSF.core.GroupControl.Actions;
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
    public unsafe partial class StopSyncedCutsceneForBattleActor : GroupControlAction
    {
        public enum TargetType
        {
            Self,
            Other,
            Both
        }
        
        [Serializable]
        public struct TargetAndState
        {
            public AssetRef<Tag> targetTag;
            public AssetRef<Tag> cutsceneTag;
        }

        public TargetAndState[] statesToSet = Array.Empty<TargetAndState>();
        
        public override void OnEnter(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            foreach (var state in statesToSet)
            {
                var targetEntity = infoEntityRef;
                if (state.targetTag != default)
                {
                    targetEntity = TaggedEntityMapping.GetEntityFromMap(frame, infoEntityRef, state.targetTag);
                }
                if (targetEntity == EntityRef.None) continue;
                StopSyncedCutsceneFor(frame, targetEntity, state.cutsceneTag);
            }
        }

        private void StopSyncedCutsceneFor(Frame frame, EntityRef battleActorRef, AssetRef<Tag> cutsceneTag)
        {
            var syncedFilter = frame.Filter<SyncedCutsceneSource>();

            var entityToRemove = default(EntityRef);
            
            while (syncedFilter.NextUnsafe(out var syncedEntity, out var syncedCutsceneSource))
            {
                if (syncedCutsceneSource->sourcePlayer != battleActorRef
                    || syncedCutsceneSource->cutsceneTag != cutsceneTag) continue;

                entityToRemove = syncedEntity;
                break;
            }
            if (entityToRemove == default) return;
            frame.Destroy(entityToRemove);
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
    internal class StopSyncedCutsceneForBattleActorNode : ActorGroupControlNode
    {
        public const string IN_PORT_Target_Tag = "TargetTag";
        public const string IN_PORT_Cutscene_Tag = "CutsceneTag";
        
        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);
            
            context.AddInputPort<Tag>(IN_PORT_Target_Tag)
                .WithDisplayName("Target Tag")
                .Build();
            
            context.AddInputPort<Tag>(IN_PORT_Cutscene_Tag)
                .WithDisplayName("Cutscene Tag")
                .Build();
        }

        public override GroupControlAction Convert()
        {
            var targetTag = ActorGroupScriptDirectorImporter.GetInputPortValue<Tag>(this.GetInputPortByName(IN_PORT_Target_Tag));
            var cutsceneTag = ActorGroupScriptDirectorImporter.GetInputPortValue<Tag>(this.GetInputPortByName(IN_PORT_Cutscene_Tag));
            
            return new StopSyncedCutsceneForBattleActor()
            {
                statesToSet = new []
                {
                    new StopSyncedCutsceneForBattleActor.TargetAndState()
                    {
                        targetTag = targetTag,
                        cutsceneTag = cutsceneTag
                    }
                }
            };
        }
    }
}
#endif