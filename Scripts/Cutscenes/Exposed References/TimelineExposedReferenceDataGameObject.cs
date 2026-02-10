using UnityEngine;

namespace HnSF
{
    [System.Serializable]
    public class TimelineExposedReferenceDataGameObject : ITimelineExposedReferenceData
    {
        public bool useGameobjectNameForID = true;
        public string name;
        public GameObject reference;

        public string GetID()
        {
            return useGameobjectNameForID ? reference.name : name;
        }

        public Object GetReference()
        {
            return reference;
        }
    }
}