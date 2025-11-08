using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class VE_Vehicle : MonoBehaviour
{
    [Title("Status")]
    public VE_StatusEnum.vE_Status status;

    [Space]
    public bool playerInVehicle;
    public bool passengerDoorsOpen = false;

    [Space]
    public BusStop currentBusStop;


    [Title("Walk surface Refrence")]
    public VehicleWalkSurface walkSurface;


    [Title("Car Controller Refrence")]
    public PG.CarController carController;

    [Title("Vehicle Refrences")]
    public GameObject entryRamp;
    public Transform cachedPlayerPositionRefrence;
    public FoldingDoorsInteractionHandler doorsInteractionHandler;

    [Space]
    [Title("Target Transforms")]
    public MoveToTargetTransform entranceTargetTransform; ///Vehicle Door entrance/exit position
    public MoveToTargetTransform isleTargetTransform; ///Vehicle Isle position, character uses this as startposition to seat destination and destination from seat during exiting

    [Space]
    [Title("Seat Assigner Refrence")]
    public VE_SeatAssigner seatAssigner; //Is responcible for assigning and unassigning seats to characters
    public PassengerCrowdBoardingHelper passengerCrowdBoardingHelper; //passanger crowd Boarding and exiting vehicle 

    [Title("Passengers")]
    public List<Passenger> passengers = new List<Passenger>();

    public static event System.Action<VE_StatusEnum.vE_Status> OnVehicleStatusChanged;
    public static event System.Action<BusStop> OnAssignedBusStop;
    //public static event System.Action OnOpenPassengerDoors;

    private void OnEnable()
    {
        doorsInteractionHandler.onDoorOpen += OnOpenDoor;
        FirstPersonController.OnFirstPersonCharacterEnterVehicle += FirstPersonController_OnFirstPersonCharacterEnterVehicle;
        carController.OnChangeGearAction += CarController_OnChangeGearAction;

    }

    private void Start()
    {
        SceneRegistry.Instance.vE_Vehicle = this;
        ///Call method to Set Entry ramp active status
        CarController_OnChangeGearAction(0);
    }



    private void CarController_OnChangeGearAction(int gear)
    {
        if (gear >= 1)
        {
            GetComponent<Rigidbody>().isKinematic = false;
            CloseDoor();
            status = VE_StatusEnum.vE_Status.MOVING;
            walkSurface.GetComponent<Collider>().enabled = true;

            entryRamp.gameObject.SetActive(false);
            OnVehicleStatusChanged?.Invoke(status);
            return;
        }

        status = VE_StatusEnum.vE_Status.STOPPED;
        entryRamp.gameObject.SetActive(true);
        GetComponent<Rigidbody>().isKinematic = true;
        walkSurface.GetComponent<Collider>().enabled = false;


        OnVehicleStatusChanged?.Invoke(status);
    }

    private void FirstPersonController_OnFirstPersonCharacterEnterVehicle(bool obj)
    {
        playerInVehicle = obj;
    }



    void OnOpenDoor(bool isOpen)
    {
        passengerDoorsOpen = isOpen;
    }


    public bool CheckForExitingPassengers()
    {
        for (int i = 0; i < passengers.Count; i++)
        {
            if (passengers[i].commute.destination == currentBusStop)
            {
                return true;
            }
        }

        return false;
    }


    [Button]
    public void SetVehicleStatus(VE_StatusEnum.vE_Status newStatus)
    {
        status = newStatus;
        OnVehicleStatusChanged(status);
    }


    public void AssignBusStop(BusStop busStop)
    {
        currentBusStop = busStop;
        OnAssignedBusStop?.Invoke(currentBusStop);
    }

    public void CloseDoor()
    {
        if (passengerDoorsOpen)
        {
            doorsInteractionHandler.CloseDoor();
        }
    }

    private void OnDisable()
    {
        doorsInteractionHandler.onDoorOpen -= OnOpenDoor;
        FirstPersonController.OnFirstPersonCharacterEnterVehicle -= FirstPersonController_OnFirstPersonCharacterEnterVehicle;
        carController.OnChangeGearAction -= CarController_OnChangeGearAction;

    }

}
