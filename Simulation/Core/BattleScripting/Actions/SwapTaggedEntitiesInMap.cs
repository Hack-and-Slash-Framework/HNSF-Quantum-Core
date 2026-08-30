using System;
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

namespace HnSF.core.GroupControl.Actions
{
    [Serializable]
    public unsafe partial class SwapTaggedEntitiesInMap : GroupControlAction
    {
        public AssetRef<Tag> entityATag;
        public AssetRef<Tag> entityBTag;
        
        public override BattleScriptResult OnEnter(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            if (!frame.Unsafe.TryGetPointer<TaggedEntityMapping>(infoEntityRef, out var taggedEntityMap))
            {
                return BattleScriptResult.Failed;
            }
            var mappingDict = frame.ResolveDictionary(taggedEntityMap->tagToEntityMap);
            
            if(!mappingDict.TryGetValue(entityATag, out var entityARef) || !mappingDict.TryGetValue(entityBTag, out var entityBRef))
            {
                return BattleScriptResult.Failed;
            }
            
            mappingDict[entityATag] = entityBRef;
            mappingDict[entityBTag] = entityARef;
            return BattleScriptResult.Succeeded;
        }
    }
}

# if UNITY_EDITOR
namespace HnSF.core.GroupControl.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(ActorGroupScriptGraph))]
    internal class SwapTaggedEntitiesInMap : ActorGroupControlNode
    {
        public const string OptionEntityATag = "EntityATag";
        public const string OptionEntityBTag = "EntityBTag";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);

            context.AddOption<AssetRef<Tag>>(OptionEntityATag)
                .WithDisplayName("Entity A Tag")
                .Build();

            context.AddOption<AssetRef<Tag>>(OptionEntityBTag)
                .WithDisplayName("Entity B Tag")
                .Build();
        }

        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);
        }

        public override GroupControlAction Convert()
        {
            this.GetNodeOptionByName(OptionEntityATag).TryGetValue(out AssetRef<Tag> entityATag);
            this.GetNodeOptionByName(OptionEntityBTag).TryGetValue(out AssetRef<Tag> entityBTag);
            
            return new Actions.SwapTaggedEntitiesInMap()
            {
                entityATag = entityATag,
                entityBTag = entityBTag
            };
        }
    }
}
#endif