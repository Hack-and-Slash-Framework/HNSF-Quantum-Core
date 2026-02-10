using Cysharp.Threading.Tasks;
using UnityEngine;

namespace HnSF.sessionhandling
{
    public class SessionHandlerBase : MonoBehaviour
    {
        public string id;
        
        public virtual bool Initialize()
        {
            return true;
        }

        public virtual void Teardown()
        {
            
        }
    }
}