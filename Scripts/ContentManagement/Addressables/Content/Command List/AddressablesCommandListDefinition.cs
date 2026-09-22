using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace HnSF
{
    [CreateAssetMenu(menuName = "HnSF/Addressables/Content/Command List Definition")]
    public partial class AddressablesCommandListDefinition : BaseCommandListDefinition
    {
        [SerializeField] protected AssetReferenceT<BaseCommandListMovesetDefinition>[] movesetRootEntries;

        [NonSerialized] protected AsyncOperationHandle<BaseCommandListMovesetDefinition>[] handles;

        public override UniTask<bool> Load(string id)
        {
            base.Load(id);
            handles = new AsyncOperationHandle<BaseCommandListMovesetDefinition>[movesetRootEntries.Length];
            return new UniTask<bool>(true);
        }

        protected override async UniTask LoadAssetsInternal()
        {
            try
            {
                for (int i = 0; i < handles.Length; i++)
                {
                    handles[i] = Addressables.LoadAssetAsync<BaseCommandListMovesetDefinition>(movesetRootEntries[i]);
                    await handles[i];
                }
                
                ReportAssetsLoadResult(true);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading command list ({name}).");
                Debug.LogException(e);
                ReportAssetsLoadResult(false);
            }
            finally
            {
                loadAssetsCompletionSource = null;
            }
        }
        
        public override BaseCommandListMovesetDefinition[] GetMovesets()
        {
            if (handles == null) return null;
            var list = new BaseCommandListMovesetDefinition[handles.Length];

            for (int i = 0; i < handles.Length; i++)
            {
                list[i] = handles[i].Result;
            }
            
            return list;
        }

        public override void UnloadAssets()
        {
            base.UnloadAssets();

            for (int i = 0; i < handles.Length; i++)
            {
                if (!handles[i].IsValid()) continue;
                Addressables.Release(handles[i]);
                handles[i] = default;
            }
        }

        public override void Unload()
        {
            base.Unload();
            handles = null;
        }
    }
}