using UnityEngine;

public class VehicleBusStopAssignTrigger : VehicleBusStopAssigner
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("VE_Vehicle"))
        {
            GetComponent<Collider>().enabled = false;
            vehicle = other.transform.root.GetComponent<VE_Vehicle>();
            vehicle.GetComponent<AIVehicleController>().isRunning = false;
            Assign();
        }
    }
}
