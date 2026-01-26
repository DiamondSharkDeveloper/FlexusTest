using Core.StateMachine;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Bootstrap
{
    /// <summary>
    /// Gameplay state starts the runtime gameplay flow.
    /// Scene objects are responsible for actual gameplay initialization.
    /// </summary>
    public sealed class GameplayState : IGameState
    {
        public UniTask Enter()
        {
            Debug.Log("GameplayState entered.");
            return UniTask.CompletedTask;
        }

        public UniTask Exit()
        {
            return UniTask.CompletedTask;
        }
    }
}