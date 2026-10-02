using UnityEngine;

namespace HnSF.sessionhandling
{
    public class SessionHandlerBase : MonoBehaviour
    {
        public string id;

        public bool TornDown { get; protected set; }

        public virtual bool Initialize()
        {
            return true;
        }

        public virtual void Teardown()
        {
            if (TornDown)
                return;
            TornDown = true;
        }
    }
}