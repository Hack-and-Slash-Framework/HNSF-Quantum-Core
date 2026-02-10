using Quantum;
using UnityEngine;

namespace HnSF
{
    [System.Serializable]
    public class TimelineExposedReferenceDataFromBindingSource : ITimelineExposedReferenceData
    {
        public string name;
        public Tag tag;

        public string GetID()
        {
            return name;
        }

        public Object GetReference()
        {
            return tag;
        }
    }
}