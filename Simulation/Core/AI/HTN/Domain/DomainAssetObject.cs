using System;
using System.Collections.Generic;
using Photon.Deterministic;
using Quantum;
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
        
        [NonSerialized] public bool remade = false;
        [NonSerialized] private IResourceManager cachedResourceManager = null;
        
        public override void Loaded(IResourceManager resourceManager, Native.Allocator allocator)
        {
            base.Loaded(resourceManager, allocator);
            cachedResourceManager = resourceManager;
            BuildRuntimeNodes(resourceManager);
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
        [ContextMenu("Force Build")]
#endif
        public void ForceBuild()
        {
            runtimeRoot = rootNode.ConvertToRuntimeObject(cachedResourceManager) as TaskRoot;
            runtimeRoot.RecursivelyAssignIDs(this, ref idCounter);
            remade = true;
        }
    }
}
