using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassengerCrowdBoardingHelper : MonoBehaviour
{
    [SerializeField] private PassengerCrowd passengerCrowd;
    [SerializeField] private float queueSpaceInterval = 1.5f;
    [SerializeField] private float moveToQueueUpDelay = .5f;
    [SerializeField] private float passengerBoardingDuration = 3f;
    [SerializeField] private float passengerExitingDuration = 3f;
    [Space]
    [SerializeField] List<MoveToSlot> queueSlots = new List<MoveToSlot>();
    [Space]
    [SerializeField] List<QueuedPassenger> queuedPassengers = new List<QueuedPassenger>();

    int currentVehicleCapacity;
    VE_Vehicle vehicle;
    BusStop currentBusStop;

    private void Start()
    {
        vehicle = GetComponent<VE_Vehicle>();
        vehicle.doorsInteractionHandler.onDoorOpen += DropOffPassengers;
    }

    private void OnEnable()
    {
        VE_Vehicle.OnAssignedBusStop += VE_Vehicle_onAssignedBusStop;
    }

    /// <summary>
    /// Triggered when the vehicle stops at a bus stop and the bus stop is assigned to the bus
    /// </summary>
    /// <param name="busStop"></param>
    private void VE_Vehicle_onAssignedBusStop(BusStop busStop)
    {
        if (busStop.GetPassengerCrowd().passengerList.Count < 1)
        {
            busStop.PopulatePassengersAtBusStop();
        }

        currentBusStop = busStop;
        Initialize();
    }


    public void Initialize()
    {

        CreateQueueSlots();

        if (vehicle.CheckForExitingPassengers())
        {
            ///Prompt player to open doors for passengers to exit
            print("<color=yellow><b>Upen door for passengers</b></color>");
            //DropOffPassengers(vehicle.passengerDoorsOpen);
            return;
        }

        if (vehicle.currentBusStop.GetPassengerCount() > 0)
        {
            print("<color=green><b>Collect passengers at bus stop</b></color>");
            return;
        }
    }


    /// <summary>
    /// Positions slots (MoveToTransforms) at interval distances
    /// </summary>
    void CreateQueueSlots()
    {
        int availableSeats = vehicle.seatAssigner.GetEmptySeatSlotCount();
        int passengersAtStop = vehicle.currentBusStop.GetPassengerCount();
        currentVehicleCapacity = availableSeats;

        if (passengersAtStop > availableSeats)
        {
            currentVehicleCapacity = (passengersAtStop - availableSeats);
        }
        else
        {
            currentVehicleCapacity = passengersAtStop;
        }

        for (int i = 0; i < currentVehicleCapacity; i++)
        {
            ///Define Lineup positions for vehicle boarding
            Vector3 newPosition = vehicle.entranceTargetTransform.transform.position + (i + 1) * queueSpaceInterval * (transform.forward * -1);
            newPosition.x += -.5f;

            queueSlots[i].isActive = true;
            queueSlots[i].position = newPosition;
            queueSlots[i].transform.position = newPosition;
        }

    }

    int i = 0;
    [Button]
    void DebugAddPassengers()
    {

        passengerCrowd.passengerList = currentBusStop.GetPassengerCrowd().passengerList;

        if (i < passengerCrowd.passengerList.Count)
        {
            AddPassenger(passengerCrowd.passengerList[i]);
            i++;
        }
    }



    #region PickPassenger
    public void AddPassenger(Passenger _passenger)
    {
        if (!passengerCrowd.passengerList.Contains(_passenger))
        {
            passengerCrowd.passengerList.Add(_passenger);
        }

        NPCharacter character = _passenger.GetCharacter();

        MoveToSlot slot = GetEmptyQueueSlot();
        slot.character = character;

        slot.onCharacterReachLocation += OnPassengerReachQueueSlot;
        character.GetMovementController().MoveToTargetLocation(slot);

        QueuedPassenger passenger = new QueuedPassenger(slot, _passenger);

        if (!queuedPassengers.Contains(passenger))
        {
            queuedPassengers.Add(passenger);
        }
    }


    void OnPassengerReachQueueSlot(NPCharacter character)
    {

        for (int i = 0; i < queuedPassengers.Count; i++)
        {
            if (queuedPassengers[i].passenger.GetCharacter() == character)
            {
                queuedPassengers[i].passenger.transform.DORotate(queuedPassengers[i].slot.transform.eulerAngles, .2f).OnComplete(() => { queuedPassengers[i].slot.isOccupied = true; BoardVehicle(); });
                return;
            }
        }
    }

    /// <summary>
    /// Check to see if there are any passengers boarding the vehicle; If none, A passenger will begin vehicle boarding
    /// </summary>
    void BoardVehicle()
    {
        if (vehicle.status == VE_StatusEnum.vE_Status.DROPPING_PASSENGERS)
        {
            print("<color=blue><b> PASSENGERS ARE EXITING... WAIT </b></color>");
            return;
        }

        for (int i = 0; i < queuedPassengers.Count; i++)
        {
            if (queuedPassengers[i].passenger.GetCharacter().state == NPC_States.NPC_STATES.BOARDING_VEH)
            {
                print("<color=cyan><b>" + queuedPassengers[i].passenger.name + "</b></color> is boarding; wait for them to reach isle");
                return;
            }

        }

        CheckForGapsInQueue(out MoveToSlot _moveToSlot, out int _emptySlotIndex);


        ///There is an empty slot; move queued passengers forward to occupy slot
        if (_moveToSlot != null)
        {
            //print("<color=red><b>DONT BOARD; SLOT IS EMPTY</b></color> is boarding; BREAK");
            //print("UPDATE QUEUE POSITIONS");

            Invoke("UpdateQueuedPassengers", .5f);

            //UpdateQueuedPassengers();
            return;
        }


        Passenger passenger = queuedPassengers[0].passenger;
        passenger.GetCharacter().GetMovementController().GetNavAgent().stoppingDistance = 0f;
        passenger.GetCharacter().GetMovementController().MoveToTargetLocation(vehicle.entranceTargetTransform);
        vehicle.entranceTargetTransform.onCharacterReachLocation += OnCharacterReachEntrance;
    }


    void OnCharacterReachEntrance(NPCharacter character)
    {
        QueuedPassenger queuedPassenger = GetQueuedPassenger(character);

        if (queuedPassenger == null)
        {
            StartCoroutine(UpdateQueueEnum());
            return;
        }

        queuedPassenger.slot.character = null;
        queuedPassenger.slot.isOccupied = false;

        for (int i = 0; i < queuedPassengers.Count; i++)
        {
            if (queuedPassengers[i].passenger.GetCharacter().state == NPC_States.NPC_STATES.BOARDING_VEH)
            {
                if (queuedPassengers[i].passenger == queuedPassenger.passenger)
                {
                    queuedPassengers.Remove(queuedPassenger);
                }
            }
        }

        if (!vehicle.doorsInteractionHandler.isOpen)
        {
            vehicle.doorsInteractionHandler.OpenDoor();
        }
    }


    bool isUpdatingQueue = false;
    IEnumerator UpdateQueueEnum()
    {
        if (isUpdatingQueue)
        {
            print("<color=yellow><b>isUpdatingQueue</b></color>");
            yield break;
        }
        isUpdatingQueue = true;
        yield return new WaitForSeconds(1f);
        UpdateQueuedPassengers();
        isUpdatingQueue = false;
    }



    /// <summary>
    /// Moves queued passangers forward in the queue 
    /// </summary>
    [Button]
    void UpdateQueuedPassengers()
    {

        int occupiableSlots = queuedPassengers.Count + 1;// Add +1 to account for the first slot 0

        for (int i = 0; i < occupiableSlots; i++)
        {
            if ((i + 1) <= queuedPassengers.Count)
            {
                queueSlots[i].character = queuedPassengers[i].slot.character;
                queuedPassengers[i].slot = queueSlots[i];
                queuedPassengers[i].passenger = queueSlots[i].character.GetComponent<Passenger>();

                queueSlots[i].character.GetMovementController().MoveToTargetLocation(queueSlots[i]);
            }


            ///Unassign character from last slot
            if (i == queuedPassengers.Count)
            {
                queueSlots[i].character = null;
                queueSlots[i].isOccupied = false;
            }
        }
    }


    void CheckForGapsInQueue(out MoveToSlot _slot, out int _emptyOccSlotIndex)
    {
        for (int i = 0; i < queueSlots.Count; i++)
        {

            if ((i + 1) < queueSlots.Count)
            {
                if (!queueSlots[i].isOccupied && queueSlots[i + 1].isOccupied)
                {
                    _slot = queueSlots[i];

                    int indx = i;

                    if (indx > queueSlots.Count)
                    {
                        indx = queueSlots.Count - 1;
                    }
                    else
                    {
                        indx = i;
                    }

                    _emptyOccSlotIndex = indx;
                    return;
                }
            }

        }

        _slot = null;
        _emptyOccSlotIndex = 0;

    }


    QueuedPassenger GetQueuedPassenger(NPCharacter character)
    {
        for (int i = 0; i < queuedPassengers.Count; i++)
        {
            if (queuedPassengers[i].passenger.GetCharacter() == character)
            {
                return queuedPassengers[i];
            }
        }

        QueuedPassenger p = null;
        return p;
    }

    MoveToSlot GetEmptyQueueSlot()
    {
        for (int i = 0; i < queueSlots.Count; i++)
        {
            if (queueSlots[i].isActive)
            {
                if (!queueSlots[i].character)
                {
                    return queueSlots[i];
                }
            }
        }

        return null;
    }
    #endregion



    #region ExitVehicle

    List<Passenger> exitingPassengers = new List<Passenger>();


    [Button("Drop Passengers at Bus Stop")]
    public void DropOffPassengers(bool doorOpen)
    {

        if (!vehicle.CheckForExitingPassengers())
        {
            if (vehicle.playerInVehicle && doorOpen)
            {
                print("'PassengerCrowdBoardingHelper', NO PASSENGERS TO DROP");
            }
            return;
        }

        vehicle.status = VE_StatusEnum.vE_Status.DROPPING_PASSENGERS;

        GetExitingPassengers();
        StartCoroutine(DropPassengersEnum());
    }


    /// <summary>
    /// Loop through passengers; get passengers that are exiting at current vehicle stop; add them to a list
    /// </summary>
    void GetExitingPassengers()
    {
        for (int i = 0; i < vehicle.passengers.Count; i++)
        {
            if (vehicle.passengers[i].commute.destination == vehicle.currentBusStop)
            {
                exitingPassengers.Add(vehicle.passengers[i]);
            }
        }
    }


    IEnumerator DropPassengersEnum()
    {
        int i = 0;
        int count = exitingPassengers.Count;

        while (i < count)
        {
            if (!exitingPassengers[i].GetComponent<NPC_VehicleBoardingHandler>())
            {
                Debug.LogError("'NPC_VehicleBoardingHandler' missing: Attach 'NPC_VehicleBoardingHandler' to Passenger character");
                break;
            }
            vehicle.passengers.Remove(exitingPassengers[i]);
            exitingPassengers[i].GetComponent<NPC_VehicleBoardingHandler>().StartExitVehicle();
            i++;
            yield return new WaitForSeconds(passengerExitingDuration);
        }

        OnPassengerDropComplete();
    }

    void OnPassengerDropComplete()
    {
        for (int i = 0; i < vehicle.passengers.Count; i++)
        {
            if (vehicle.passengers[i].commute.destination == vehicle.currentBusStop)
            {
                print("Exiting passengers waiting");
                return;
            }
        }

        vehicle.status = VE_StatusEnum.vE_Status.READY_FOR_BOARDING;

        if (queuedPassengers.Count > 0)
        {
            BoardVehicle();
        }
    }

    #endregion

    private void OnDisable()
    {
        VE_Vehicle.OnAssignedBusStop -= VE_Vehicle_onAssignedBusStop;
        vehicle.doorsInteractionHandler.onDoorOpen -= DropOffPassengers;
    }
}


[System.Serializable]
public class QueuedPassenger
{
    public MoveToSlot slot;
    public Passenger passenger;

    public QueuedPassenger(MoveToSlot _moveToSlot, Passenger _passenger)
    {
        slot = _moveToSlot;
        passenger = _passenger;
    }
}
