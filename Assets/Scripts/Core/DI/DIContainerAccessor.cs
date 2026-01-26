namespace Project.Core.DI
{
    /// <summary>
    /// Provides access to the application's DI container for scene-level composition roots.
    /// This keeps scene objects independent from the bootstrap MonoBehaviour instance.
    /// </summary>
    public static class DIContainerAccessor
    {
        public static DIContainer Container { get; private set; }

        public static void Set(DIContainer container)
        {
            Container = container;
        }
    }
}