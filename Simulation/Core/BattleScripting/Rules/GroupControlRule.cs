using System;
using Quantum;

namespace HnSF.core.GroupControl.Grabbers
{
    [Serializable]
    public unsafe partial class GroupControlRule
    {
        public string Label;
        public bool disable;

        public virtual bool IsValid(Frame frame, EntityRef infoEntityRef, ref BattleScriptContext context)
        {
            return true;
        }
    }
}