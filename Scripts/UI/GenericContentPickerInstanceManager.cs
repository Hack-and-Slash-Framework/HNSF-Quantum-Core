using UnityEngine;

namespace HnSF.ui
{
    public class GenericContentPickerInstanceManager : MonoBehaviour
    {
        public GenericContentPickerInstance instancePrefab;
        
        public GenericContentPickerInstance CreateInstance<T>(Transform parent) where T : IContentDefinition
        {
            var instance = GameObject.Instantiate(instancePrefab, parent, false);
            return instance;
        }
    }
}