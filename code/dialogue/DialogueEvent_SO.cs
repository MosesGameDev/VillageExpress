using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue Event", menuName = "Dialogue/Dialogue Event")]
public class DialogueEvent_SO : ScriptableObject
{
    public static event Action<string> PassengerBoardVehicleEvent;

    public void TriggerPassengerBoardVehicleEvent(string passengerID)
    {
        PassengerBoardVehicleEvent?.Invoke(passengerID);
    }
}
