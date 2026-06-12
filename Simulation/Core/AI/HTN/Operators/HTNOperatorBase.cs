using System;
using HnSF.core.AI.HTN.Conditions;
using HnSF.core.AI.HTN.Effects;
using HnSF.core.GroupControl;
using HnSF.core.GroupControl.Grabbers;
using HnSF.core.systems;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
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
        public HTNConditionBase[] preconditions = Array.Empty<HTNConditionBase>();
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public HTNEffectBase[] effects = Array.Empty<HTNEffectBase>();
        
        public NextExecutedNodeType nextExecutedNodeLogic;
        public int[] nextNodesOrdered;
        public WeightedList<int> nextNodesWeighted;

        public virtual void OnEnter(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            
        }
        
        public virtual bool Tick(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            return false;
        }
        
        public virtual void OnExit(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            
        }
    }
}