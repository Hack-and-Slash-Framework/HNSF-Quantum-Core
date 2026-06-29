#if UNITY_EDITOR
using System;
using HnSF.core.AI.HTN;
using HnSF.core.AI.HTN.Conditions;
using HnSF.core.AI.HTN.Tasks;
using HnSF.core.GroupControl.Actions;
using Quantum;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace HnSF.core.AI.HTN.Domain
{
    [Serializable]
    [UseWithGraph(typeof(HTNDomainGraph))]
    public class PrimitiveTaskFromAssetObjectNode : TaskNodeBase
    {
        public const string optionAssetObject = "PTAssetObject";
        
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);

            context.AddOption<AssetRef<PrimitiveTaskAssetObject>>(optionAssetObject)
                .WithDisplayName("Asset Object")
                .Build();
        }

        public override ITask Convert()
        {
            GetNodeOptionByName(optionAssetObject).TryGetValue<AssetRef<PrimitiveTaskAssetObject>>(out var assetRef);
            
            var task = new PrimitiveTaskFromAssetObject();
            task.Weight = GetWeight();
            task.assetObjectRef = assetRef;
            return task;
        }
    }
}
#endif