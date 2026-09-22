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
    public unsafe partial class GetTargetingEntity : GroupControlFunctionEntityRef
    {
        public override EntityRef Execute(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            if (!frame.Unsafe.TryGetPointer<BattleActorAI>(infoEntityRef, out var battleActorAI)
                || !frame.Unsafe.TryGetPointer<EntityTargeting>(battleActorAI->aiActorRef, out var targeting))
                return EntityRef.None;

            return targeting->target;
        }
    }
}

# if UNITY_EDITOR
namespace HnSF.core.GroupControl.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(ActorGroupScriptGraph))]
    internal class FunctionGetTargetingEntity : FunctionNodeBase
    {
        public const string inEntityTag = "EntityTag";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
        }

        protected override void OnDefinePorts(Node.IPortDefinitionContext context)
        {
            AddInputOutputExecutionPorts(context);
        }

        public override GroupControlFunction Convert()
        {
            return new Functions.GetTargetingEntity()
            {
            };
        }
    }
}
#endif