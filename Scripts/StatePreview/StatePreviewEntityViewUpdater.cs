using Quantum;
using UnityEditor;
using UnityEngine;

namespace HnSF
{
    public class StatePreviewEntityViewUpdater : QuantumEntityViewUpdater
    {
        public PreviewRenderUtility RenderUtility;
        public GameObject rootObject;
        
        protected override QuantumEntityView CreateEntityViewInstance(EntityView asset, Vector3? position = null, Quaternion? rotation = null)
        {
            Assert.Check(asset.Prefab != null);
            var viewPrefab = asset.Prefab.GetComponent<QuantumEntityView>();

            if (Pool != null) {
                var instance = Pool.Create(viewPrefab);

                if (position.HasValue == true) {
                    instance.transform.position = position.Value;
                }

                if (rotation.HasValue == true) {
                    instance.transform.rotation = rotation.Value;
                }

                return instance;
            } else
            {
                var instance = GameObject.Instantiate(viewPrefab.gameObject, rootObject.transform, false);
                if (position.HasValue && rotation.HasValue) instance.transform.SetPositionAndRotation(position.Value, rotation.Value);
                instance.gameObject.hideFlags = HideFlags.HideAndDontSave;
                
                EditorActorUpdateHelper.DoAwake(instance);
                
                return instance.GetComponent<QuantumEntityView>();
            }
        }

        protected override void DestroyEntityViewInstance(QuantumEntityView instance)
        {
            EditorActorUpdateHelper.DoDisable(instance.gameObject);
            
            if (Pool != null) {
                Pool.Destroy(instance);
            } else {
                DestroyImmediate(instance.gameObject);
            }
        }
    }
}