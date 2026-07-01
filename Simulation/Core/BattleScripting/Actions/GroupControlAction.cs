using System;
using HnSF.core.GroupControl.Grabbers;
using HnSF.core.systems;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
#endif

namespace HnSF.core.GroupControl.Actions
{
    [Serializable]
#if QUANTUM_UNITY
    [MovedFrom(autoUpdateAPI: false, sourceNamespace: "HnSF.core.scripting.VersusIntro.Actions", sourceClassName: "VersusScriptAction")]
#endif
    public unsafe partial class GroupControlAction
    {
        public string Label;
        public bool disable;
        public bool endExecution;
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public GroupControlRule[] rules = Array.Empty<GroupControlRule>();
        
        public NextExecutedNodeType nextExecutedNodeLogic;
        public int[] nextNodesOrdered;
        public WeightedList<int> nextNodesWeighted;

        public virtual bool IsValid(Frame frame, EntityRef entityRef, ref BattleScriptContext battleScriptContext)
        {
            foreach (var rule in rules)
            {
                if (!rule.IsValid(frame, entityRef, ref battleScriptContext))
                    return false;
            }
            return true;
        }
        
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