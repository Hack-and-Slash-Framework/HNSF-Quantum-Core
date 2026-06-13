using System.Collections.Generic;
using HnSF.core.AI.HTN;
using HnSF.core.AI.HTN.Conditions;
#if QUANTUM_UNITY
using UnityEngine;
#if UNITY_EDITOR
using HnSF.core.AI.HTN.Effects;
using UnityEditor;
#endif
#endif

namespace Quantum
{
    public static class HTNHelpers
    {
#if UNITY_EDITOR
        public static bool EditorEffectsListsEqual(IReadOnlyList<IEffect> currentEffects, IReadOnlyList<IEffect> generatedEffects)
        {
            if (ReferenceEquals(currentEffects, generatedEffects))
                return true;

            if (currentEffects == null || generatedEffects == null)
                return false;

            if (currentEffects.Count != generatedEffects.Count)
                return false;

            for (int i = 0; i < currentEffects.Count; i++)
            {
                if (!EditorEffectsEqual(currentEffects[i], generatedEffects[i]))
                    return false;
            }

            return true;
        }
        
        public static bool EditorConditionsListsEqual(IReadOnlyList<ICondition> currentOperators, IReadOnlyList<ICondition> generatedOperators)
        {
            if (ReferenceEquals(currentOperators, generatedOperators))
                return true;

            if (currentOperators == null || generatedOperators == null)
                return false;

            if (currentOperators.Count != generatedOperators.Count)
                return false;

            for (int i = 0; i < currentOperators.Count; i++)
            {
                if (!EditorConditionsEqual(currentOperators[i], generatedOperators[i]))
                    return false;
            }

            return true;
        }
        
        public static bool EditorActionListsEqual(IReadOnlyList<HTNOperatorBase> currentOperators, IReadOnlyList<HTNOperatorBase> generatedOperators)
        {
            if (ReferenceEquals(currentOperators, generatedOperators))
                return true;

            if (currentOperators == null || generatedOperators == null)
                return false;

            if (currentOperators.Count != generatedOperators.Count)
                return false;

            for (int i = 0; i < currentOperators.Count; i++)
            {
                if (!EditorActionsEqual(currentOperators[i], generatedOperators[i]))
                    return false;
            }

            return true;
        }
        
        public static bool EditorEffectsEqual(IEffect currentCondition, IEffect generatedCondition)
        {
            if (ReferenceEquals(currentCondition, generatedCondition))
                return true;

            if (currentCondition == null || generatedCondition == null)
                return false;

            if (currentCondition.GetType() != generatedCondition.GetType())
                return false;

            return EditorJsonUtility.ToJson(currentCondition) == EditorJsonUtility.ToJson(generatedCondition);
        }
        
        public static bool EditorActionsEqual(HTNOperatorBase currentOperator, HTNOperatorBase generatedOperator)
        {
            if (ReferenceEquals(currentOperator, generatedOperator))
                return true;

            if (currentOperator == null || generatedOperator == null)
                return false;

            if (currentOperator.GetType() != generatedOperator.GetType())
                return false;

            return EditorJsonUtility.ToJson(currentOperator) == EditorJsonUtility.ToJson(generatedOperator);
        }
        
        public static bool EditorConditionsEqual(ICondition currentCondition, ICondition generatedCondition)
        {
            if (ReferenceEquals(currentCondition, generatedCondition))
                return true;

            if (currentCondition == null || generatedCondition == null)
                return false;

            if (currentCondition.GetType() != generatedCondition.GetType())
                return false;

            return EditorJsonUtility.ToJson(currentCondition) == EditorJsonUtility.ToJson(generatedCondition);
        }
#endif
    }
}
