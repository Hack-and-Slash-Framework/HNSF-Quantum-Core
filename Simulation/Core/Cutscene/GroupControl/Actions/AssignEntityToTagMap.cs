using System;
using HnSF.core.GroupControl.Actions;
using Photon.Deterministic;
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
    public unsafe partial class AssignEntityToTagMap : GroupControlAction
    {
        public AssetRef<Tag> tag;
        
        public override void OnEnter(Frame frame, EntityRef infoEntityRef)
        {
            frame.AddOrGet<TaggedEntityMapping>(infoEntityRef, out var tem);
            var mappingDict = frame.ResolveDictionary(tem->tagToEntityMap);
            
            var gamemodeParticipantsGlobal = frame.Unsafe.GetOrAddSingletonPointer<GamemodeParticipantsGlobal>();
            var participantDataEntities = frame.ResolveDictionary(gamemodeParticipantsGlobal->participantDataEntities);
            
            var firstParticipantInfoEntity = participantDataEntities[1];
            
            var participantBattleActorEntities = frame.Unsafe.GetPointer<ParticipantDataBattleActorEntities>(firstParticipantInfoEntity);
            var participantActorEntityList = frame.ResolveList(participantBattleActorEntities->battleActorEntities);

            if (participantActorEntityList.Count == 0) return;
            mappingDict[tag] = participantActorEntityList[0];
        }
        
        public override bool Tick(Frame frame, EntityRef infoEntityRef)
        {
            return true;
        }
        
        public override void OnExit(Frame frame, EntityRef infoEntityRef)
        {
        }
    }
}

# if UNITY_EDITOR
namespace HnSF.core.GroupControl.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(ActorGroupScriptGraph))]
    internal class AssignEntityToTagMap : ActorGroupControlNode
    {
        public const string OPTION_CAMERA_TAG = "Tag";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);

            context.AddOption<AssetRef<Tag>>(OPTION_CAMERA_TAG)
                .WithDisplayName("Tag")
                .Build();
        }

        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);
        }

        public override GroupControlAction Convert()
        {
            this.GetNodeOptionByName(OPTION_CAMERA_TAG).TryGetValue(out AssetRef<Tag> tag);
            
            
            return new Actions.AssignEntityToTagMap()
            {
                tag = tag,
            };
        }
    }
}
#endif