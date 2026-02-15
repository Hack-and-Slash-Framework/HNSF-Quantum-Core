using UnityEngine;

namespace HnSF.ui
{
    public class GenericContentPickerInstanceManager : MonoBehaviour
    {
        public static GenericContentPickerInstanceManager instance;
        
        public GenericContentPickerInstance instancePrefab;
        
        public void Awake()
        {
            if (instance != null)
            {
                GameObject.Destroy(gameObject);
                return;
            }
            instance = this;
        }

        public GenericContentPickerInstance CreateInstance<T>(Transform parent) where T : IContentDefinition
        {
            var instance = GameObject.Instantiate(instancePrefab, parent, false);
            return instance;
        }
    }
}