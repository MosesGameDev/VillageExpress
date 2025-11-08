using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Assigns 'BusStop' to vehicle, once assigned passengers can be dropped off and picked by vehicle
/// </summary>
public class VehicleBusStopAssigner : MonoBehaviour
{
    [SerializeField, ReadOnly] private VE_StatusEnum.vE_Status vehicleStatus;

    [Space]
    public BusStop busStop;
    public VE_Vehicle vehicle;

    /// <summary>
    /// Assign bus stop passangers to bustop passenger handler, calls 'OnAssignedBusStop' event; subscriber 'PassengerCrowdBoardingHelper'
    /// </summary>
    [Button("Assign Bus Stop to Vehicle")]
    public void Assign()
    {

        if (!vehicle.passengerCrowdBoardingHelper)
        {
            print("<color=red>'<b>PassengerCrowdBoardingHelper</b>' not assigned in '<b>VE_Vehicle</b>'</color>");
            return;
        }

        vehicle.status = VE_StatusEnum.vE_Status.STOPPED;
        vehicle.AssignBusStop(busStop);
        vehicleStatus = vehicle.status;

    }

}
