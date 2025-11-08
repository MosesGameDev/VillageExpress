using UnityEngine;

public class WaypointUIElementVisibilityController : MonoBehaviour
{
    public string displayText;

    [Space]
    [SerializeField] private bool vehicleInteriorWaypoint;
    [Space]
    [SerializeField] private Waypoint_Indicator waypointIndicator;

    WaypointUIE uie;

    private void OnEnable()
    {
        FirstPersonController.OnFirstPersonCharacterEnterVehicle += FirstPersonController_OnFirstPersonCharacterEnterVehicle;
    }

    private void OnDisable()
    {
        FirstPersonController.OnFirstPersonCharacterEnterVehicle -= FirstPersonController_OnFirstPersonCharacterEnterVehicle;
    }
    private void Start()
    {

        ToggleEnableTracking(SceneRegistry.Instance.player.firstPersonController.isInVehicle);
    }


    private void FirstPersonController_OnFirstPersonCharacterEnterVehicle(bool inVehicle)
    {
        ToggleEnableTracking(inVehicle);
    }

    void ToggleEnableTracking(bool inVehicle)
    {

        if (vehicleInteriorWaypoint)
        {
            if (inVehicle)
            {
                waypointIndicator.enableStandardTracking = true;
            }
            else
            {
                waypointIndicator.enableStandardTracking = false;
            }

        }
        else
        {
            if (!inVehicle)
            {
                waypointIndicator.enableStandardTracking = true;
            }
            else
            {
                waypointIndicator.enableStandardTracking = false;
            }
        }

    }

}
