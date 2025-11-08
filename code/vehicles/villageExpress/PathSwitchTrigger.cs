using PG;
using UnityEngine;


public class PathSwitchTrigger : MonoBehaviour
{
    [SerializeField] private AIPath path;
    [SerializeField] private AIVehicleController vehicleController;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("VE_Vehicle"))
        {
            vehicleController.AIPath = path;
        }
    }


}
