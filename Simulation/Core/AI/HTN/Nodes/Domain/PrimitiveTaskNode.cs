#if UNITY_EDITOR
using System;
using HnSF.core.AI.HTN;
using HnSF.core.AI.HTN.Conditions;
using HnSF.core.AI.HTN.Nodes;
using HnSF.core.AI.HTN.Tasks;
using HnSF.core.GroupControl.Actions;
using Unity.GraphToolkit.Editor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HnSF.core.AI.HTN.Domain
{
    [Serializable]
    [UseWithGraph(typeof(HTNDomainGraph))]
    public class PrimitiveTaskNode : TaskNodeBase
    {
        public const string optionTaskGraph = "PTAssetObject";
        
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
            context.AddOption<Object>(optionTaskGraph)
                .WithDisplayName("Graph")
                .Build();
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            base.OnDefinePorts(context);
        }

        public override ITask Convert()
        {
            GetNodeOptionByName(optionTaskGraph).TryGetValue(out Object taskGraph);
            if (taskGraph == null) return null;
            var task = PrimitiveTaskGraphImporter.ConvertFromAsset(taskGraph);
            task.Weight = GetWeight();
            return task;
        }
    }
}
#endif