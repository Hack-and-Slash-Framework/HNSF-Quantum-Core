using System;
using UnityEngine;

namespace HnSF
{
    public class GameObjectRendererList : MonoBehaviour
    {
        public Renderer[] renderers = Array.Empty<Renderer>();
        
        public virtual bool IsVisible()
        {
            foreach (var rnd in renderers)
            {
                if (rnd.isVisible) return true;
            }
            return false;
        }
    }
}