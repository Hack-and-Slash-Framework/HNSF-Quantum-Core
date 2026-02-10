using UnityEngine;

namespace HnSF
{
    [System.Serializable]
    public unsafe class CutsceneBindingSourceCondition
    {
        public virtual bool Decide(Object wantedBinding)
        {
            return true;
        }
    }
}