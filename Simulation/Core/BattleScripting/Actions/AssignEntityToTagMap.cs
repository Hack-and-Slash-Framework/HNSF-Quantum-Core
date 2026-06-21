using System;
using HnSF.core.GroupControl.Actions;
using HnSF.core.GroupControl.Functions;
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
        public GroupControlFunctionEntityRef entityRefFunction;
        public bool clearTagIfEntityNotFound;
        
        public override void OnEnter(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            frame.AddOrGet<TaggedEntityMapping>(infoEntityRef, out var tem);
            var mappingDict = frame.ResolveDictionary(tem->tagToEntityMap);
            var entityRef = entityRefFunction.Execute(frame, infoEntityRef);
            if (clearTagIfEntityNotFound && (entityRef == EntityRef.None || !frame.Exists(entityRef)))
            {
                mappingDict[tag] = EntityRef.None;
                return;
            }

            mappingDict[tag] = entityRef;
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
    internal class AssignEntityToTagMap : ActorGroupControlNode
    {
        public const string optionMapTag = "Tag";
        public const string inFunctionEntityRef = "EntityRefFunction";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);

            context.AddOption<AssetRef<Tag>>(optionMapTag)
                .WithDisplayName("Tag")
                .Build();
        }

        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);

            context.AddInputPort(inFunctionEntityRef)
                .WithDisplayName("Entity Ref Function")
                .Build();
        }

        public override GroupControlAction Convert()
        {
            this.GetNodeOptionByName(optionMapTag).TryGetValue(out AssetRef<Tag> tag);
            
            
            return new Actions.AssignEntityToTagMap()
            {
                tag = tag,
                entityRefFunction = ConvertFunctionNode<GroupControlFunctionEntityRef>(GetInputPortByName(inFunctionEntityRef))
            };
        }
    }
}
#endif