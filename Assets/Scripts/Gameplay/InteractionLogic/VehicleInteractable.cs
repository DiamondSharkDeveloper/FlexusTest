using Gameplay.VehicleLogic;
using UnityEngine;

namespace Gameplay.InteractionLogic
{
    /// <summary>
    /// Interactable that toggles vehicle enter/exit.
    /// If player is not in a vehicle - enters this one.
    /// If player is in this vehicle - exits.
    /// </summary>
    public sealed class VehicleInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private VehicleRoot vehicleRoot;

        public string Prompt
        {
            get
            {
                return "Press E to enter/exit";
            }
        }

        private void Awake()
        {
            if (vehicleRoot == null)
                vehicleRoot = GetComponentInParent<VehicleRoot>();
        }

        public void Interact(InteractionContext context)
        {
            PlayerControlService control = context.ControlService;
            if (control == null)
                return;

            VehicleRoot vehicle = vehicleRoot;

            if (vehicle == null)
                return;

            if (!control.IsInVehicle())
            {
                control.EnterVehicle(vehicle);
                return;
            }

            VehicleRoot current = control.CurrentVehicle();
            if (current == vehicle)
            {
                control.ExitVehicle();
            }
        }
    }
}
