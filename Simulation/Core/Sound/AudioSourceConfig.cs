#if QUANTUM_UNITY
using UnityEngine;
#endif

namespace Quantum
{
    public partial class AudioSourceConfig : AssetObject
    {
#if QUANTUM_UNITY
        public AudioSource prefab;
        public float defaultMinDistance = 0;
        public float defaultMaxDistance = 10;
        
        public AudioSource CreateAudioSource(Vector3 position)
        {
            var go = GameObject.Instantiate(prefab, position, Quaternion.identity);
            go.minDistance = defaultMinDistance;
            go.maxDistance = defaultMaxDistance;
            return go;
        }        
#endif
    }
}