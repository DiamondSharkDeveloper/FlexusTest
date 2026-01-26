using Gameplay.CameraLogic;
using Gameplay.InputLogic;
using UnityEngine;

namespace Gameplay.Control
{
    /// <summary>
    /// Coordinates which entity is currently controlled by the player.
    /// Routes input to the active controllable and updates camera follow target.
    /// </summary>
    public sealed class ControlSwitcher
    {
        private readonly InputRouter inputRouter;
        private readonly ICameraRig cameraRig;

        private IControllable active;
        private IControllable character;
        private IControllable vehicle;

        private Transform characterCameraTarget;
        private Transform vehicleCameraTarget;

        public ControlSwitcher(InputRouter inputRouter, ICameraRig cameraRig)
        {
            this.inputRouter = inputRouter;
            this.cameraRig = cameraRig;
        }

        public void RegisterCharacter(IControllable controllable, Transform cameraTarget)
        {
            character = controllable;
            characterCameraTarget = cameraTarget;
        }

        public void RegisterVehicle(IControllable controllable, Transform cameraTarget)
        {
            vehicle = controllable;
            vehicleCameraTarget = cameraTarget;
        }

        public void SetCharacterActive()
        {
            SwitchTo(character, characterCameraTarget);
        }

        public void SetVehicleActive()
        {
            SwitchTo(vehicle, vehicleCameraTarget);
        }

        private void SwitchTo(IControllable next, Transform cameraTarget)
        {
            if (next == null)
                return;

            if (active != null)
                active.DisableControl();

            active = next;
            active.EnableControl();

            inputRouter.SetPrimaryConsumer(active);

            if (cameraTarget != null)
                cameraRig.SetTarget(cameraTarget);
        }
    }
}