namespace Project.Core.DI
{
    // Path: Assets/_Project/Scripts/Core/DI/IInstaller.cs
    // Purpose: Optional pattern - split bindings into multiple installers.
    // Works with: Later we can have CoreInstaller, GameplayInstaller, etc.

    public interface IInstaller
    {
        void Install(DIContainer container);
    }
}