using System;
using UnityEngine;
using UnityEngine.Events;

public class LocationVehicleTrigger : MonoBehaviour
{
    public Location location;

    [Space]
    public bool initializeBusStopSpawnersInLocation;

    public UnityEvent onEnterTrigger;
    public UnityEvent onExitTrigger;

    public static event Action<Location> OnEnterLocation;
    public static event Action<Location> OnExitLocation;

    bool inTrigger;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("VE_Vehicle"))
        {
            if (inTrigger)
            {
                return;
            }

            if (!location.visited)
            {
                OnEnterLocation?.Invoke(location);

                if (initializeBusStopSpawnersInLocation)
                {
                    location.busStop.PopulatePassengersAtBusStop();
                }

                onEnterTrigger.Invoke();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("VE_Vehicle"))
        {
            OnExitLocation?.Invoke(location);
            onExitTrigger.Invoke();
        }
    }
}
