using System;
using System.Collections.Generic;
using HnSF.core.GroupControl.Actions;
using HnSF.core.GroupControl.Grabbers;
using HnSF.core.state.actions;
using HnSF.core.state.decisions;
using Photon.Deterministic;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
#endif


namespace HnSF.core.GroupControl
{
    [MovedFrom(autoUpdateAPI: true, sourceNamespace: "HnSF.core.scripting.VersusIntro", sourceClassName: "BattleActorVersusIntroScript")]
    public class BattleActorGroupControlScript : AssetObject
    {
        public AssetRef<BattleActorDefinition> vsTarget;
        
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public GroupControlRule[] entityGrabActions = Array.Empty<GroupControlRule>();
        
#if QUANTUM_UNITY
        [SerializeReference, SubclassSelector]
#endif
        public List<GroupControlAction> actions = new List<GroupControlAction>();
        
        public virtual bool RulesValid(Frame frame, EntityRef infoEntityRef)
        {
            foreach (var r in entityGrabActions)
            {
                if (!r.IsValid(frame, infoEntityRef)) return false;
            }
            return true;
        }
    }
}