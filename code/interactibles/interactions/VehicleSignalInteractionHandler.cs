using UnityEngine;
using UnityEngine.Events;

public class VehicleSignalInteractionHandler : Interactible
{
    public enum Signal { MOVE, STOP };
    [SerializeField] private Signal signal;
    [SerializeField] private AIVehicleController vehicle;



    [Space]
    public UnityEvent onSignalVehicle;

    Interactible _interactible;

    private void Start()
    {
        OnStartInteraction += OnInteract;
        OnEndInteraction += OnInteractionEnd;
    }

    private void OnEnable()
    {
        VE_Vehicle.OnVehicleStatusChanged += VE_Vehicle_OnVehicleStatusChanged;
    }

    private void VE_Vehicle_OnVehicleStatusChanged(VE_StatusEnum.vE_Status status)
    {
        switch (status)
        {
            case VE_StatusEnum.vE_Status.MOVING:
                signal = Signal.STOP;
                instruction = "Stop Vehicle";
                break;
            case VE_StatusEnum.vE_Status.STOPPED:
                signal = Signal.MOVE;
                instruction = "Start Moving";
                break;
        }
    }

    private void OnDisable()
    {
        VE_Vehicle.OnVehicleStatusChanged -= VE_Vehicle_OnVehicleStatusChanged;
    }

    void OnInteract(Interactible interactible)
    {
        _interactible = interactible;

        switch (signal)
        {
            case Signal.MOVE:
                StartMoving();
                print("VehicleSignalInteractionHandler: MOVE signal sent to vehicle");
                break;

            case Signal.STOP:
                StopMoving();
                print("VehicleSignalInteractionHandler: STOP signal sent to vehicle");
                break;
        }

        OnEndInteraction.Invoke(_interactible);
    }


    void OnInteractionEnd(Interactible interactible)
    {
        PlayerInteractionsHandler.instance.RenableInteractions(interactible);
    }

    public void StartMoving()
    {
        vehicle.isRunning = true;
    }

    public void StopMoving()
    {
        vehicle.isRunning = false;
    }

}
