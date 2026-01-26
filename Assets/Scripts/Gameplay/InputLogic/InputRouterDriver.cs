using UnityEngine;

namespace Project.Gameplay.Input
{
    /// <summary>
    /// Unity update loop bridge for InputRouter.
    /// Keeps InputRouter as a pure C# class and still updates it every frame.
    /// </summary>
    public sealed class InputRouterDriver : MonoBehaviour
    {
        private InputRouter router;

        public void Construct(InputRouter router)
        {
            this.router = router;
        }

        private void Update()
        {
            if (router == null)
                return;

            router.Tick();
        }
    }
}