using System;
using System.Collections.Generic;
using HnSF;
using HnSF.core.AI.HTN.Tasks;
using HnSF.core.GroupControl;
using HnSF.core.GroupControl.Actions;
using HnSF.core.GroupControl.Grabbers;
using Photon.Deterministic;
using Quantum;
using UnityEngine.Scripting.APIUpdating;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF.core.AI.HTN.Tasks
{
    public class DomainAssetObject : AssetObject, ITaskIDSource
    {
        [NonSerialized] private byte idCounter;
        public Dictionary<byte, ITask> IdToTask { get; } = new();
        public Dictionary<ITask, byte> taskToId { get; } = new();

        [NonSerialized] public TaskRoot runtimeRoot = null;
        
        public TaskRoot rootNode = new TaskRoot();
        
        public override void Loaded(IResourceManager resourceManager, Native.Allocator allocator)
        {
            base.Loaded(resourceManager, allocator);
            if (runtimeRoot == null) BuildRuntimeNodes(resourceManager);
        }

        private void BuildRuntimeNodes(IResourceManager resourceManager)
        {
            Log.Debug("Built runtime node.");
            runtimeRoot = rootNode.ConvertToRuntimeObject(resourceManager) as TaskRoot;
            runtimeRoot.RecursivelyAssignIDs(this, ref idCounter);
        }
        
        private void OnValidate()
        {
            
        }

#if QUANTUM_UNITY
        [ContextMenu("Bake")]
#endif
        public void Bake()
        {
            
        }
    }
}
