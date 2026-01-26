using Cysharp.Threading.Tasks;

namespace Core.Services
{
    // Path: Assets/_Project/Scripts/Core/Services/IAssetProvider.cs
    // Purpose: One place to load/release assets (Addressables now, maybe something else later ).

    public interface IAssetProvider
    {
        UniTask<T> Load<T>(string address) where T : class;
        UniTask Release(object asset);
    }
}