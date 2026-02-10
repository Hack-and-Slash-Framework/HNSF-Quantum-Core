using System;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine.Scripting.APIUpdating;
#endif

namespace HnSF.core.GroupControl.Actions
{
    [Serializable]
#if QUANTUM_UNITY
    [MovedFrom(autoUpdateAPI: true, sourceNamespace: "HnSF.core.scripting.VersusIntro.Actions", sourceClassName: "VersusScriptAction")]
#endif
    public unsafe partial class GroupControlAction
    {
        public string Label;
        public bool disable;

        public virtual void OnEnter(Frame frame, EntityRef infoEntityRef)
        {
            
        }
        
        public virtual bool Tick(Frame frame, EntityRef infoEntityRef)
        {
            return false;
        }
        
        public virtual void OnExit(Frame frame, EntityRef infoEntityRef)
        {
            
        }
    }
}