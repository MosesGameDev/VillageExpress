using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VE_SeatAssigner : MonoBehaviour
{
    [SerializeField] private PlayerSeatInteraction currentInteractibleSeat;
    public SeatObject[] seats;



    /// <summary>
    /// Get total number of seat slots in the Vehicle
    /// </summary>
    /// <returns></returns>
    public int GetTotalSeatSlotCount()
    {
        int count = 0;

        for (int i = 0; i < seats.Length; i++)
        {
            for (int j = 0; j < seats[i].seatSlots.Length; j++)
            {
                count++;
            }
        }

        return count;
    }

    public List<SeatObjectSlot> GetSeatSlots()
    {
        List<SeatObjectSlot> seatObjectSlots = new List<SeatObjectSlot>();

        for (int i = 0; i < seats.Length; i++)
        {
            for (int j = 0; j < seats[i].seatSlots.Length; j++)
            {
                if (seats[i].seatSlots[j].slotStatus == SeatObjectSlot.SeatSlotStatus.EMPTY)
                {
                    seatObjectSlots.Add(seats[i].seatSlots[j]);
                }
            }
        }

        return seatObjectSlots;
    }


    /// <summary>
    /// Get total number of 'EMPTY'seat slots in the Vehicle
    /// </summary>
    /// <returns></returns>

    public int GetEmptySeatSlotCount(out SeatObject _seat)
    {
        int count = 0;
        SeatObject __seat = null;
        for (int i = 0; i < seats.Length; i++)
        {
            for (int s = 0; s < seats[i].seatSlots.Length; s++)
            {
                if (seats[i].seatSlots[s].slotStatus == SeatObjectSlot.SeatSlotStatus.EMPTY)
                {
                    count++;
                    _seat = seats[i];
                }
            }
        }
        _seat = __seat;
        return count;
    }

    public int GetEmptySeatSlotCount()
    {
        int count = 0;
        for (int i = 0; i < seats.Length; i++)
        {
            for (int s = 0; s < seats[i].seatSlots.Length; s++)
            {
                if (seats[i].seatSlots[s].slotStatus == SeatObjectSlot.SeatSlotStatus.EMPTY)
                {
                    count++;
                }
            }
        }
        return count;
    }



    /// <summary>
    /// Loops through the seats array and finds a seat with an empty seat slot,
    /// random bool to shuffles array before looping to get seat
    /// </summary>
    /// <param name="_emptySeatSlot"></param>
    /// <param name="random"></param>
    /// <returns></returns>
    SeatObject GetEmptySeat(out SeatObjectSlot _emptySeatSlot, bool random = false)
    {
        SeatObject[] _seatsArray = seats;

        if (random)
        {
            List<SeatObject> _seatObjects = _seatsArray.ToList();
            _seatObjects.Shuffle();
            _seatsArray = _seatObjects.ToArray();
        }

        for (int i = 0; i < _seatsArray.Length; i++)
        {
            for (int s = 0; s < _seatsArray[i].seatSlots.Length; s++)
            {
                if (_seatsArray[i].seatSlots[s].slotStatus == SeatObjectSlot.SeatSlotStatus.EMPTY)
                {
                    _emptySeatSlot = _seatsArray[i].seatSlots[s];
                    return _seatsArray[i];
                }
            }
        }

        _emptySeatSlot = null;
        return null;
    }



    /// <summary>
    /// Loops through Seats and gets an empty one and assigns it to the character
    /// character moves to the assinged seat via Tween
    /// character seats on tween Complete
    /// </summary>
    /// <param name="_character"></param>
    /// <param name="getRandomSeat"></param>
    /// <param name="_slot"></param>
    /// <param name="_seat"></param>
    public void AssignSeatToCharacter(NPCharacter _character, bool getRandomSeat, out SeatObjectSlot _slot, out SeatObject _seat)
    {
        _seat = getRandomSeat ? GetEmptySeat(out _slot, getRandomSeat) : null;

        _seat = GetEmptySeat(out _slot, getRandomSeat);
        Transform chrTransform = _character.transform;

        Sequence sequence = DOTween.Sequence();
        float chrSpeed = _character.GetMovementController().GetNavAgent().speed;
        float distanceTo = Vector3.Distance(_character.transform.position, _seat.entryRefrencePositionTransform.transform.position);
        float duration = distanceTo / chrSpeed;

        Vector3 lookAt = _seat.entryRefrencePositionTransform.transform.position;
        lookAt.y = _character.transform.position.y;

        SeatObject __seat = _seat;

        sequence
        .Append(chrTransform.DOLookAt(lookAt, 0.5f))
        .Append
        (
            chrTransform.DOMove(_seat.entryRefrencePositionTransform.transform.position, duration)
            .OnStart(() => { _character.GetMovementController().AnimatorSetSpeed(.1f); })
            .OnComplete(() => { _character.GetMovementController().AnimatorSetSpeed(0); })
        )
        .OnComplete
        (
            () =>
            {
                __seat.MoveToSeatSlotPosition(_character);
            }
        );
    }

}
