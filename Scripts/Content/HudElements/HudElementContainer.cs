using UnityEngine;

namespace HnSF
{
    public class HudElementContainer : MonoBehaviour
    {
        [SerializeField] private GameObject container;
        
        public GameObject GetElementInstance(Transform parent)
        {
            return GameObject.Instantiate(container, parent, false);
        }
    }
}