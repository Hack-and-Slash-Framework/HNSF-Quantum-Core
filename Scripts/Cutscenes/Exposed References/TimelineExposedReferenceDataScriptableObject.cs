using UnityEngine;

namespace HnSF
{
    [System.Serializable]
    public class TimelineExposedReferenceDataScriptableObject : ITimelineExposedReferenceData
    {
        public bool useSoNameForID;
        public string name;
        public ScriptableObject reference;

        public string GetID()
        {
            return useSoNameForID ? reference.name : name;
        }

        public Object GetReference()
        {
            return reference;
        }
    }
}