using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeatAssigner : MonoBehaviour
{
    [SerializeField] private NPCharacter character;
    [SerializeField] private SeatObject seat;

    [Space]
    [SerializeField] private bool autoAssignOnStart = false;


    IEnumerator Start()
    {
        if (autoAssignOnStart)
        {
            yield return new WaitForSeconds(1f);
            AssignSeatToCharacter();
        }
    }

    [Button("Assign Seat To Character")]
    public void AssignSeatToCharacter()
    {
        if(character == null || seat == null)
        {
            print("<color=red> character or seat unassigned </color>");
            return;
        }

        if (!seat.isThereAnEmptySlotInSeat())
        {
            print("<color=red> Seat is already Occupied </color>");
            return;
        }
        character.proceduralAnimController.DisableProcedurals();

        character.GetMovementController().MoveToTargetLocation(seat.entryRefrencePositionTransform);
    }

    public void AssignSeatToCharacter(NPCharacter _character, SeatObject _seat)
    {
        character.GetMovementController().MoveToTargetLocation(seat.entryRefrencePositionTransform);
    }
}
