using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Core.Services
{
    // Path: Assets/_Project/Scripts/Core/Services/SceneLoader.cs
    // Purpose: Default scene loading implementation (single mode).

    public sealed class SceneLoader : ISceneLoader
    {
        public async UniTask LoadSingle(string sceneName)
        {
            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single).ToUniTask();
        }
    }
}