using UnityEngine;

public class BusRouteManager : MonoBehaviour
{

    [Space]
    public float minDistanceCost = 10;

    [Space]
    public Location currentLocation;
    public Location nextLocation;

    [Space]
    public Location[] busRouteLocations;

    int locationIndex;

    #region Events
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
        currentLocation = location;
        print($"<color=green>BUS ROUTE MANAGER</color>, <b>ENTERING LOCATION:</b> {location.locationName}");

        if (locationIndex < busRouteLocations.Length - 1)
        {
            locationIndex++;

            if (locationIndex > busRouteLocations.Length - 1)
            {
                return;
            }

            nextLocation = busRouteLocations[locationIndex];
        }
    }

    private void LocationVehicleTrigger_OnExitLocation(Location location)
    {
        print("<color=green>BUS ROUTE MANAGER</color>, <b>EXITING LOCATION:</b> " + location.name);
    }
    #endregion


    public int CalculateCommuteCost(Location startLocation, Location endLocation)
    {
        int startIndex = GetLocationIndex(startLocation);
        int endIndex = GetLocationIndex(endLocation);
        if (startIndex > endIndex)
        {
            return 0;
        }
        float totalDistance = 0f;
        // Iterate through each segment between start and end locations
        for (int i = startIndex; i < endIndex; i++)
        {
            float segmentDistance = Vector3.Distance(busRouteLocations[i].transform.position, busRouteLocations[i + 1].transform.position);
            totalDistance += segmentDistance;
        }
        // Calculate the total cost based on the total distance
        float totalCost = (minDistanceCost) * (totalDistance / 100);

        // Round the total cost to the nearest 5
        int roundedCost = Mathf.RoundToInt(totalCost / 10) * 10;
        return roundedCost;
    }



    public int GetLocationIndex(Location location)
    {
        for (int i = 0; i < busRouteLocations.Length; i++)
        {
            if (busRouteLocations[i] == location) return i;
        }

        return 0;
    }


    public Location GetRandomLocation(int startIndex)
    {
        if (startIndex >= busRouteLocations.Length)
        {
            print("OUT of Bounds");
            return null;
        }

        int i = startIndex + 1;
        if (i >= busRouteLocations.Length)
        {
            i = busRouteLocations.Length - 1;
        }
        return busRouteLocations[Random.Range(i, busRouteLocations.Length)];
    }



}
