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

namespace HnSF.core.GroupControl.Functions
{
    [Serializable]
    public unsafe partial class GetEntityFromTagMap : GroupControlFunctionEntityRef
    {
        public BattleScriptingParamAssetRef tagAssetRef;
        
        public override EntityRef Execute(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            if (!frame.Unsafe.TryGetPointer(infoEntityRef, out TaggedEntityMapping* tem))
                return EntityRef.None;

            var map = frame.ResolveDictionary(tem->tagToEntityMap);

            if (map.TryGetValue(tagAssetRef.Resolve(frame, infoEntityRef, ref context).Id, out var gotEntityRef))
                return gotEntityRef;
            return EntityRef.None;
        }
    }
}

# if UNITY_EDITOR
namespace HnSF.core.GroupControl.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(ActorGroupScriptGraph))]
    internal class FunctionGetEntityFromTagMap : FunctionNodeBase
    {
        public const string inEntityTag = "EntityTag";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
        }

        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);
            
            context.AddInputPort(inEntityTag)
                .WithDisplayName("Entity Tag Param")
                .Build();
        }

        public override GroupControlFunction Convert()
        {
            return new Functions.GetEntityFromTagMap()
            {
                tagAssetRef = GetInputPortParam<BattleScriptingParamAssetRef, AssetRef>(GetInputPortByName(inEntityTag)),
            };
        }
    }
}
#endif