using FIMSpace.FProceduralAnimation;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AddPassengersToVehicle : MonoBehaviour
{
    [SerializeField, Range(0, 15)] private int numberOfPassengers;
    [SerializeField] private BusStop destinationBusStop;

    VE_Vehicle vehicle;

    [Button("Add passengers to Vehicle")]
    public void AddPassengers()
    {
        vehicle = SceneRegistry.Instance.vE_Vehicle;
        List<SeatObjectSlot> slots = vehicle.seatAssigner.GetSeatSlots();

        for (int i = 0; i < numberOfPassengers; i++)
        {
            Passenger passenger = SceneRegistry.Instance.passangerSpawner.ObjectPool.Get();
            passenger.passengerStatus = PassengerStatusEnum.PassengerStatus.IN_TRANSIT;
            SeatObjectSlot slot = slots[i];
            slot.owningSeat = slots[i].owningSeat;

            StartCoroutine(SetUpPassengerWithDelay(passenger, slot));
        }
    }

    IEnumerator SetUpPassengerWithDelay(Passenger passenger, SeatObjectSlot slot)
    {
        slot.slotStatus = SeatObjectSlot.SeatSlotStatus.OCCUPIED;
        slot.passenger = passenger;
        passenger.GetCharacter().vehicleSeatingSlot = slot;
        passenger.transform.SetParent(slot.owningSeat.transform);

        // Set initial position
        passenger.transform.position = slot.targetPosition.transform.position;
        passenger.transform.eulerAngles = slot.targetPosition.transform.eulerAngles;



        // Wait for 0.6 seconds
        yield return new WaitForSeconds(.65f);

        passenger.GetCharacter().proceduralAnimController.DisableBoneSimulators();
        passenger.GetCharacter().proceduralAnimController.DisableLookAnimator();
        passenger.GetCharacter().proceduralAnimController.DisableTrigger();

        passenger.GetCharacter().GetComponent<NavMeshAgent>().enabled = false;
        passenger.GetCharacter().GetComponent<LegsAnimator>().enabled = false;



        // Set final position with the y offset after delay
        Vector3 pos = slot.targetPosition.transform.position;
        passenger.transform.position = pos;

        passenger.GetCharacter().GetComponent<Animator>().SetBool("isSitting", true);
        passenger.GetCharacter().state = NPC_States.NPC_STATES.SITTING;

        passenger.commute.destination = destinationBusStop;
        vehicle.passengers.Add(passenger);
    }
}