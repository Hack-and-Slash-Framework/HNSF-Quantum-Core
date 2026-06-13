using System;
using System.Collections.Generic;
using HnSF.core.AI.HTN.Conditions;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF.core.AI.HTN.Tasks
{
    [Serializable]
    public abstract class CompoundTask : ICompoundTask
    {
        public string Label
        {
            get => label; set => label = value;
        }

        public byte ID
        {
            get => id; set => id = value;
        }
        public ICompoundTask Parent { get; set; }
        public List<ICondition> Conditions
        {
            get => conditionals;
            set => conditionals = value;
        }

        [SerializeField] protected byte id;
        [SerializeField] protected string label;
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        protected List<ICondition> conditionals = new List<ICondition>();
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        protected List<ITask> subtasks = new List<ITask>();
        
        public virtual bool IsValid(ref HTNAgentContext context)
        {
            foreach (var condition in conditionals)
            {
                var result = condition.IsValid(ref context);

                if (!result)
                    return false;
            }
            return true;
        }
        
        public DecompositionStatus OnIsValidFailed(ref HTNAgentContext context)
        {
            return DecompositionStatus.Failed;
        }

        public DecompositionStatus Decompose(ref HTNAgentContext context, byte startIndex, out Queue<byte> result)
        {
            var status = OnDecompose(ref context, startIndex, out result);
            return status;
        }

        protected abstract DecompositionStatus OnDecompose(ref HTNAgentContext context, byte startIndex,
            out Queue<byte> result);
        protected abstract DecompositionStatus OnDecomposeTask(ref HTNAgentContext context, ITask task, byte taskIndex,
            int[] oldStackDepth, out Queue<byte> result);
        protected abstract void OnDecomposePrimitiveTask(ref HTNAgentContext context, IPrimitiveTask task,
            byte taskIndex, int[] oldStackDepth, out Queue<byte> result);
        protected abstract DecompositionStatus OnDecomposeCompoundTask(ref HTNAgentContext context, ICompoundTask task,
            byte taskIndex, int[] oldStackDepth, out Queue<byte> result);
        //protected abstract DecompositionStatus OnDecomposeSlot(Frame frame, EntityRef infoEntityRef, Slot task, int taskIndex, int[] oldStackDepth, out Queue<ITask> result);

        public virtual ITask ConvertToRuntimeObject(IResourceManager resourceManager)
        {
            return null;
        }

        public void RecursivelyAssignIDs(ITaskIDSource idSource, ref byte id)
        {
            ID = ++id;
            idSource.IdToTask.Add(ID, this);
            idSource.taskToId.Add(this, ID);
            
            foreach(var subtask in subtasks)
                subtask.RecursivelyAssignIDs(idSource, ref id);
        }
        
        public virtual void FillOtherWithValues(CompoundTask other, IResourceManager resourceManager)
        {
            other.label = label;
            other.conditionals = new List<ICondition>(conditionals);
            other.subtasks = new List<ITask>(subtasks);
            for (int i = 0; i < subtasks.Count; i++)
                other.subtasks[i] = subtasks[i] == null ? null : subtasks[i].ConvertToRuntimeObject(resourceManager);
        }
    }
}
