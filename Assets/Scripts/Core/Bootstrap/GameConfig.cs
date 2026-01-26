using UnityEngine;

namespace Project.Core.Bootstrap
{
    [CreateAssetMenu(menuName = "Project/Game Config")]
    public sealed class GameConfig : ScriptableObject
    {
        public string GameplaySceneName = "Gameplay";
    }
}