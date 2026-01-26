using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Core.StateMachine
{
    // Path: Assets/_Project/Scripts/Core/StateMachine/GameStateMachine.cs
    // Purpose: Switch between game states (Bootstrap -> Gameplay, etc).
    // Works with: BootstrapState/GameplayState and any future states.
    // Notes:only basic functions for now

    public sealed class GameStateMachine
    {
        private readonly Dictionary<Type, IGameState> _states = new();
        private IGameState _current;

        public void Register<T>(T state) where T : class, IGameState
        {
            _states[typeof(T)] = state;
        }

        public async UniTask SwitchTo<T>() where T : class, IGameState
        {
            if (_current != null)
                await _current.Exit();

            _current = (T)_states[typeof(T)];
            await _current.Enter();
        }
    }
}