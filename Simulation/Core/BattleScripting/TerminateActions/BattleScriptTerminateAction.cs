using System;
using Quantum;
#if QUANTUM_UNITY
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
#endif

namespace HnSF.core.GroupControl.TerminateActions
{
    [Serializable]
    public unsafe partial class BattleScriptTerminateAction
    {
        public string Label;
        
        public virtual void Execute(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context, BattleScriptResult result)
        {
            
        }
    }
}