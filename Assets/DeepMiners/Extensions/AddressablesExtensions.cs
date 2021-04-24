using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace DeepMiners.Extensions
{
    public static class AddressablesExtensions
    {
        public static async Task<T> LoadOrGetAsync<T>(this AssetReference assetReference) where T : Object
        {
            AsyncOperationHandle op = assetReference.OperationHandle;
            if (op.IsValid()) 
            {
                AsyncOperationHandle<T> handle = op.Convert<T>();
                while (handle.IsDone == false)
                {
                    await Task.Yield();
                }
                return handle.Result;
            }
            return await assetReference.LoadAssetAsync<T>().Task;
        }
        
        public static async Task<T> LoadOrGetComponentAsync<T>(this AssetReference assetReference) where T : Component
        {
            AsyncOperationHandle op = assetReference.OperationHandle;
            if (op.IsValid()) 
            {
                AsyncOperationHandle<GameObject> handle = op.Convert<GameObject>();
                while (handle.IsDone == false)
                {
                    await Task.Yield();
                }

                if (handle.Result == null)
                {
                    return null;
                }
                
                return handle.Result.GetComponent<T>();
            }
            GameObject go = await assetReference.LoadAssetAsync<GameObject>().Task;
            if (go == null)
            {
                return null;
            }
            return go.GetComponent<T>();
        }
    }
}