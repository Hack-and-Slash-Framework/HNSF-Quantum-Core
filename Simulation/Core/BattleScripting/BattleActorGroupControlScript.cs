using System;
using System.Collections.Generic;
using HnSF.core.GroupControl.Actions;
using HnSF.core.GroupControl.Grabbers;
using HnSF.core.GroupControl.TerminateActions;
using HnSF.core.state.actions;
using HnSF.core.state.decisions;
using Photon.Deterministic;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;
#endif


namespace HnSF.core.GroupControl
{
    [MovedFrom(autoUpdateAPI: true, sourceNamespace: "HnSF.core.scripting.VersusIntro", sourceClassName: "BattleActorVersusIntroScript")]
    public unsafe partial class BattleActorGroupControlScript : AssetObject
    {
        public AssetRef<BattleActorDefinition> vsTarget;
        
#if QUANTUM_UNITY
        [FormerlySerializedAs("entityGrabActions")] [SerializeReference, SubclassSelector]
#endif
        public GroupControlRule[] conditions = Array.Empty<GroupControlRule>();
        
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public List<GroupControlAction> actions = new List<GroupControlAction>();
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public List<BattleScriptTerminateAction> onCompleteActions = new();
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public List<BattleScriptTerminateAction> onFailActions = new();
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public List<BattleScriptTerminateAction> onCancelActions = new();
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public List<BattleScriptTerminateAction> onTerminateActions = new();
        
        
        public virtual bool RulesValid(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            foreach (var r in conditions)
            {
                if (!r.IsValid(frame, infoEntityRef, ref context)) return false;
            }
            return true;
        }
    }
}