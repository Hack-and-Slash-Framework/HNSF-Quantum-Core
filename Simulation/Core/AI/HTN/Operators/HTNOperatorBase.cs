using System;
using System.Collections.Generic;
using HnSF.core.AI.HTN.Conditions;
using HnSF.core.AI.HTN.Effects;
using HnSF.core.GroupControl;
using HnSF.core.GroupControl.Grabbers;
using HnSF.core.systems;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace HnSF.core.AI.HTN
{
    [Serializable]
    public unsafe partial class HTNOperatorBase
    {
        public string Label;
        public bool disable;
        public bool endExecution;
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public List<ICondition> preconditions = new List<ICondition>();
        
        public NextExecutedNodeType nextOperatorSelectionType;
        public int[] nextOperatorsOrdered;
        public WeightedList<int> nextOperatorsWeighted;

        public virtual bool IsValid(ref HTNAgentContext context)
        {
            foreach (var precondition in preconditions)
            {
                if (!precondition.IsValid(ref context))
                    return false;
            }
            return true;
        }
        
        public virtual HTNTaskStatus OnEnter(ref HTNAgentContext context)
        {
            return HTNTaskStatus.Executing;
        }
        
        public virtual HTNTaskStatus Tick(ref HTNAgentContext context)
        {
            return HTNTaskStatus.Success;
        }
        
        public virtual void OnExit(ref HTNAgentContext context)
        {
            
        }

        public virtual void OnAbort(ref HTNAgentContext context)
        {
            OnExit(ref context);
        }
    }
}