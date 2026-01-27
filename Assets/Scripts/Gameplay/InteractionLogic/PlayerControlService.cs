using Gameplay.CharacterLogic;
using Gameplay.Control;
using Gameplay.VehicleLogic;
using UnityEngine;

namespace Gameplay.InteractionLogic
{
    /// <summary>
    /// Coordinates entering and exiting vehicles.
    /// Keeps seat/exit rules in one place and drives ControlSwitcher.
    /// </summary>
    public sealed class PlayerControlService
    {
        private readonly ControlSwitcher controlSwitcher;

        private CharacterRoot character;
        private VehicleRoot currentVehicle;

        public PlayerControlService(ControlSwitcher controlSwitcher)
        {
            this.controlSwitcher = controlSwitcher;
        }

        public void RegisterCharacter(CharacterRoot character)
        {
            this.character = character;
        }

        public bool IsInVehicle()
        {
            return currentVehicle != null;
        }

        public VehicleRoot CurrentVehicle()
        {
            return currentVehicle;
        }

        public void EnterVehicle(VehicleRoot vehicle)
        {
            if (vehicle == null || character == null)
                return;

            if (currentVehicle != null)
                return;

            currentVehicle = vehicle;

            Transform seat = vehicle.DriverSeat != null ? vehicle.DriverSeat : vehicle.transform;

            character.transform.position = seat.position;
            character.transform.rotation = seat.rotation;

            character.gameObject.SetActive(false);

            controlSwitcher.RegisterVehicle(vehicle, vehicle.CameraTarget);
            controlSwitcher.SetVehicleActive();
        }

        public void ExitVehicle()
        {
            if (currentVehicle == null || character == null)
                return;

            Transform exit = currentVehicle.ExitPoint != null ? currentVehicle.ExitPoint : currentVehicle.transform;

            character.gameObject.SetActive(true);

            Vector3 safePos = exit.position;
            safePos.y += 0.1f;

            character.transform.position = safePos;
            character.transform.rotation = Quaternion.Euler(0f, exit.rotation.eulerAngles.y, 0f);

            controlSwitcher.SetCharacterActive();

            currentVehicle = null;
        }
    }
}
