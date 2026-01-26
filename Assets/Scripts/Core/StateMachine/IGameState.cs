using Cysharp.Threading.Tasks;

namespace Project.Core.StateMachine
{
    // Path: Assets/_Project/Scripts/Core/StateMachine/IGameState.cs
    // Purpose: Simple async-friendly state interface.
    // Works with: GameStateMachine calls Enter/Exit.
    // Notes: UniTask keeps allocations low and plays nicely in Unity.

    public interface IGameState
    {
         UniTask Enter();
         UniTask Exit();
    }
}