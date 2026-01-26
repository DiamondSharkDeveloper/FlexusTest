using System;
using System.Collections.Generic;

namespace Core.DI
{
    // Path: Assets/_Project/Scripts/Core/DI/DIContainer.cs
    // Purpose: Tiny DI container: bind singletons/transients and resolve them by type.
    // Works with: Project Composition Root binds everything here on startup.
    // Notes: Keep it simple

    public sealed class DIContainer
    {
        private readonly Dictionary<Type, Func<DIContainer, object>> _factories = new();
        private readonly Dictionary<Type, object> _singletons = new();

        // Bind an already created singleton instance.
        public void BindSingleton<T>(T instance) where T : class
        {
            _singletons[typeof(T)] = instance;
        }

        // Bind a singleton created lazily via factory.
        public void BindSingleton<T>(Func<DIContainer, T> factory) where T : class
        {
            _factories[typeof(T)] = c => GetOrCreateSingleton(typeof(T), () => factory(c));
        }

        // Bind transient: every Resolve returns a new instance.
        public void BindTransient<T>(Func<DIContainer, T> factory) where T : class
        {
            _factories[typeof(T)] = c => factory(c);
        }

        public T Resolve<T>() where T : class => (T)Resolve(typeof(T));

        public object Resolve(Type type)
        {
            if (_singletons.TryGetValue(type, out var single))
                return single;

            if (_factories.TryGetValue(type, out var factory))
                return factory(this);

            throw new InvalidOperationException($"DI: no binding for type {type.FullName}");
        }

        private object GetOrCreateSingleton(Type type, Func<object> create)
        {
            if (_singletons.TryGetValue(type, out var single))
                return single;

            var instance = create();
            _singletons[type] = instance;
            return instance;
        }
    }
}