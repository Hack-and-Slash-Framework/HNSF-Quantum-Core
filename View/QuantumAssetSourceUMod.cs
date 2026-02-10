#if HNSF_UMOD
using System;
using Object = UnityEngine.Object;
using System.Runtime.ExceptionServices;
using UMod;

namespace Quantum
{
    /// <summary>
    /// UMod-based implementation of the asset source pattern.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public partial class QuantumAssetSourceUMod<T> where T : UnityEngine.Object
    {
        /// <summary>
        /// Resource mod name.
        /// </summary>
        public string ResourceModName;
        
        /// <summary>
        /// Resource asset path.
        /// </summary>
        public string ResourcePath;

        /// <summary>
        /// Sub-object name. If empty, the main object is loaded.
        /// </summary>
        public string SubObjectName;

        [NonSerialized] private object _state;
        [NonSerialized] private int _acquireCount;

        /// <summary>
        /// Loads the asset. In synchronous mode, the asset is loaded immediately. In asynchronous mode, the asset is loaded in the background.
        /// </summary>
        /// <param name="synchronous"></param>
        public void Acquire(bool synchronous)
        {
            if (_acquireCount == 0)
            {
                LoadInternal(synchronous);
            }

            _acquireCount++;
        }

        /// <summary>
        /// Unloads the asset. If the asset is not loaded, an exception is thrown. If the asset is loaded multiple times, it is only
        /// unloaded when the last acquire is released.
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void Release()
        {
            if (_acquireCount <= 0)
            {
                throw new Exception("Asset is not loaded");
            }

            if (--_acquireCount == 0)
            {
                UnloadInternal();
            }
        }

        /// <summary>
        /// Returns <see langword="true"/> if the asset is loaded.
        /// </summary>
        public bool IsCompleted
        {
            get
            {
                if (_state == null)
                {
                    // hasn't started
                    return false;
                }

                if (_state is ModAsyncOperation asyncOp && !asyncOp.IsDone)
                {
                    // still loading, wait
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Blocks until the asset is loaded. If the asset is not loaded, an exception is thrown.
        /// </summary>
        /// <returns>The loaded asset</returns>
        public T WaitForResult()
        {
            Assert.Check(_state != null);
            if (_state is ModAsyncOperation asyncOp)
            {
                if (asyncOp.IsDone)
                {
                    FinishAsyncOp(asyncOp);
                }
                else
                {
                    // just load synchronously, then pass through
                    _state = null;
                    LoadInternal(synchronous: true);
                }
            }

            if (_state == null)
            {
                throw new InvalidOperationException(
                    $"Failed to load asset {typeof(T)}: {ResourcePath}[{SubObjectName}]. Asset is null.");
            }

            if (_state is T asset)
            {
                return asset;
            }

            if (_state is ExceptionDispatchInfo exception)
            {
                exception.Throw();
                throw new NotSupportedException();
            }

            throw new InvalidOperationException(
                $"Failed to load asset {typeof(T)}: {ResourcePath}, SubObjectName: {SubObjectName}");
        }

        private void FinishAsyncOp(ModAsyncOperation asyncOp)
        {
            try
            {
                var asset = string.IsNullOrEmpty(SubObjectName)
                    ? asyncOp.Result
                    : LoadNamedResource(ResourcePath, SubObjectName);
                if (asset)
                {
                    _state = asset;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Missing Resource: {ResourcePath}, SubObjectName: {SubObjectName} ModName: {ResourceModName}");
                }
            }
            catch (Exception ex)
            {
                _state = ExceptionDispatchInfo.Capture(ex);
            }
        }

        private void LoadInternal(bool synchronous)
        {
            Assert.Check(_state == null);
            try
            {
                if (synchronous)
                {
                    _state = string.IsNullOrEmpty(SubObjectName)
                        ? LoadNamedResource(ResourceModName, ResourcePath)
                        : LoadNamedResource(ResourceModName, ResourcePath, SubObjectName);
                }
                else
                {
                    _state = LoadNamedResourceAsync(ResourceModName, ResourcePath);
                }

                if (_state == null)
                {
                    _state = new InvalidOperationException(
                        $"Missing Resource: {ResourcePath}, SubObjectName: {SubObjectName} ModName: {ResourceModName}");
                }
            }
            catch (Exception ex)
            {
                _state = ExceptionDispatchInfo.Capture(ex);
            }
        }

        private void UnloadInternal()
        {
            if (_state is ModAsyncOperation asyncOp)
            {
                /*
                asyncOp.completed += op =>
                {
                    // unload stuff
                };*/
            }
            else if (_state is Object)
            {
                // unload stuff
            }

            _state = null;
        }

        /// <summary>
        /// The description of the asset source. Used for debugging.
        /// </summary>
        public string Description =>
            $"Resource: [{ResourceModName}]{ResourcePath}{(!string.IsNullOrEmpty(SubObjectName) ? $"[{SubObjectName}]" : "")}";

#if UNITY_EDITOR
        public T EditorInstance => null;
#endif
        
        private static T LoadNamedResource(string resourceModName, string resourcePath, string subObjectName)
        {
            var host = Mod.GetLoadedMod(resourceModName);
            if (host == null) return null;

            var assets = host.Assets.LoadWithSubAssets<T>(resourcePath);

            for (int i = 0; i < assets.Length; i++)
            {
                var asset = assets[i];
                if(asset is null) continue;
                if (string.Equals(asset.name, subObjectName, StringComparison.Ordinal))
                {
                    return asset;
                }
            }

            return null;
        }
        
        private static T LoadNamedResource(string resourceModName, string resourcePath)
        {
            var host = Mod.GetLoadedMod(resourceModName);
            if (host == null) return null;
            return host.Assets.Load<T>(resourcePath);
        }
        
        private static ModAsyncOperation<T> LoadNamedResourceAsync(string resourceModName, string resourcePath)
        {
            var host = Mod.GetLoadedMod(resourceModName);
            if (host == null) return null;
            return host.Assets.LoadAsync<T>(resourcePath);
        }
    }
}
#endif