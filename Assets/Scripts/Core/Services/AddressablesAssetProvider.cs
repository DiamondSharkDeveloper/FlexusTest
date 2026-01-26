using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Core.Services
{
    // Path: Assets/_Project/Scripts/Core/Services/AddressablesAssetProvider.cs
    // Purpose: Wrap Addressables API, track handles so we can release cleanly.

    public sealed class AddressablesAssetProvider : IAssetProvider
    {
        private readonly Dictionary<object, AsyncOperationHandle> _handles = new();

        public async UniTask<T> Load<T>(string address) where T : class
        {
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);
            T asset = await handle.ToUniTask();
            _handles[asset] = handle;
            return asset;
        }

        public UniTask Release(object asset)
        {
            if (asset == null)
            {
                return UniTask.CompletedTask;
            }
            
            if (_handles.TryGetValue(asset, out var handle))
            {
                Addressables.Release(handle);
                _handles.Remove(asset);
            }

            return UniTask.CompletedTask;
        }
    }
}