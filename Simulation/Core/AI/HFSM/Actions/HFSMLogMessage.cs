using System;

namespace Quantum.HFSM.Actions
{
    [Serializable]
    public unsafe partial class HFSMLogMessage : AIAction
    {
        public string message;
        
        public override void Execute(Frame frame, EntityRef entity, ref AIContext aiContext)
        {
            Log.Debug(message);
        }
    }
}