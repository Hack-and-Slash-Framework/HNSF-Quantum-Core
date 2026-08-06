using System;
using System.Collections.Generic;
using HnSF.core.AI.HTN.Conditions;
using HnSF.core.AI.HTN.Effects;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF.core.AI.HTN.Tasks
{
    [Serializable]
    public unsafe partial class PrimitiveTask : IPrimitiveTask
    {
        public byte ID
        {
            get => id;
            set => id = value;
        }

        public int Weight
        {
            get => weight;
            set => weight = value;
        }

        public string Label
        {
            get => label;
            set => label = value;
        }

        public ICompoundTask Parent { get; set; }
        public List<ICondition> Conditions
        {
            get => conditions;
            set => conditions = value;
        }

        public List<ICondition> ExecutingConditions
        {
            get => executingConditions;
            set => executingConditions = value;
        }

        public List<HTNOperatorBase> Operators
        {
            get => operators;
            set => operators = value;
        }

        public List<IEffect> Effects
        {
            get => effects;
            set => effects = value;
        }

        [SerializeField] private byte id;
        [SerializeField] private string label;
        [SerializeField] protected int weight = 1;
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        private List<ICondition> conditions = new List<ICondition>();
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        private List<ICondition> executingConditions = new List<ICondition>();
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public List<HTNOperatorBase> operators = new List<HTNOperatorBase>();
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public List<IEffect> effects = new List<IEffect>();

        public DecompositionStatus OnIsValidFailed(ref HTNAgentContext context)
        {
            return DecompositionStatus.Failed;
        }

        public virtual bool IsValid(ref HTNAgentContext context)
        {
            foreach (var condition in conditions)
            {
                var result = condition.IsValid(ref context);

                if (!result)
                    return false;
            }

            return true;
        }

        public virtual ITask ConvertToRuntimeObject(IResourceManager resourceManager)
        {
            var copy = new PrimitiveTask();
            FillOtherWithValues(copy, resourceManager);
            return copy;
        }

        public void RecursivelyAssignIDs(ITaskIDSource idSource, ref byte id)
        {
            ID = ++id;
            
            idSource.IdToTask.Add(ID, this);
            idSource.taskToId.Add(this, ID);
        }

        public virtual void FillOtherWithValues(PrimitiveTask other, IResourceManager resourceManager)
        {
            other.Label = Label;
            other.weight = weight;
            other.conditions = new List<ICondition>(conditions);
            other.executingConditions = new List<ICondition>(executingConditions);
            other.operators = new List<HTNOperatorBase>(operators);
            other.effects = new List<IEffect>(effects);
        }

        public void ApplyEffects(ref HTNAgentContext context)
        {
            foreach (var effect in Effects)
            {
                effect.Apply(ref context);
            }
        }

        public void Stop(ref HTNAgentContext context)
        {
            if (context.agent->currentPlan.currentOperator < 0
                || context.agent->currentPlan.currentOperator >= Operators.Count)
                return;
            
            Operators[context.agent->currentPlan.currentOperator].OnExit(ref context);
        }

        public void Abort(ref HTNAgentContext context)
        {
            if (context.agent->currentPlan.currentOperator < 0
                || context.agent->currentPlan.currentOperator >= Operators.Count)
                return;
            
            Operators[context.agent->currentPlan.currentOperator].OnAbort(ref context);
        }
    }
}