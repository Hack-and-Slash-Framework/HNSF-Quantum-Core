#if UNITY_EDITOR
using System;
using HnSF.core.AI.HTN;
using HnSF.core.AI.HTN.Nodes;
using HnSF.core.GroupControl;
using Photon.Deterministic;
using Quantum;
using Unity.GraphToolkit.Editor;

namespace HnSF.Nodes
{
    [Serializable]
    [UseWithGraph(typeof(PrimitiveTaskGraph), typeof(ActorGroupScriptGraph), typeof(HTNDomainGraph))]
    public class ConstantHTNWorldStateBoolean : Node, IConstantNode
    {
        public const string inVariableInt = "var";
        
        public Type DataType => typeof(byte);
        
        public bool TrySetValue<T>(T value)
        {
            return false;
        }

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);

            context.AddOption<bool>(inVariableInt)
                .WithDisplayName("Value")
                .Build();
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            base.OnDefinePorts(context);

            context.AddOutputPort(NodeHelper.EXECUTION_PORT_DEFAULT_NAME)
                .WithDisplayName("Out")
                .Build();
        }

        public bool TryGetValue<T>(out T value)
        {
            value = default(T);
            var gotValue = GetNodeOptionByName(inVariableInt).TryGetValue<bool>(out var v);
            if (!gotValue)
                return false;
            if (typeof(T) != DataType)
                return false;
            value = (T)(object)(v ? (byte)1 : (byte)0);
            return true;
        }
    }
}
#endif