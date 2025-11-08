using UnityEngine;

public class Location : MonoBehaviour
{
    public string locationName;
    [Multiline]
    public string locationDescriptions;

    [Space]
    public bool visited;
    public bool isStatingLocation;
    public bool isFinalDestination;

    [Space]
    public BusStop busStop;


    ///Points of intrest ...
    ///Objectives ....


    #region EVENTS

    private void OnEnable()
    {
        LocationVehicleTrigger.OnEnterLocation += LocationVehicleTrigger_OnEnterLocation;
        LocationVehicleTrigger.OnExitLocation += LocationVehicleTrigger_OnExitLocation;
    }

    private void OnDisable()
    {
        LocationVehicleTrigger.OnEnterLocation -= LocationVehicleTrigger_OnEnterLocation;
        LocationVehicleTrigger.OnExitLocation -= LocationVehicleTrigger_OnExitLocation;

    }

    private void LocationVehicleTrigger_OnEnterLocation(Location location)
    {
        if (location == this)
        {
            visited = true;

            if (busStop == null)
            {
                return;
            }

            busStop.PopulatePassengersAtBusStop();
        }
    }

    private void LocationVehicleTrigger_OnExitLocation(Location location)
    {
        //throw new System.NotImplementedException();
    }
    #endregion


    public void Start()
    {
        if (SceneRegistry.Instance.busRouteManager != null)
        {
            visited = false;
        }
    }

}
