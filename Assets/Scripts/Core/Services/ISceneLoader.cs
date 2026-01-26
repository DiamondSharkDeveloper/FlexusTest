using Cysharp.Threading.Tasks;

namespace Core.Services
{
    // Path: Assets/_Project/Scripts/Core/Services/ISceneLoader.cs

    public interface ISceneLoader
    {
        UniTask LoadSingle(string sceneName);
    }
}