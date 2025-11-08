using DG.Tweening;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class SeatObject : MonoBehaviour
{
    public enum SeatPosition { LEFT = 0, MIDDLE = 1, RIGHT = 2 }

    public SeatPosition seatPosition;

    /// <summary>
    /// Determines Character position before starting seating Action
    /// </summary>
    public MoveToTargetTransform entryRefrencePositionTransform;


    /// <summary>
    /// Slots (isOccupied=true) onCharacter seat, (isOccupied=False) onCharacterStans
    /// </summary>
    public SeatObjectSlot[] seatSlots;
    SeatObjectSlot emptySlot = null;


    private void Start()
    {
        Initialize();
    }

    private void OnEnable()
    {
        PlayerGrabPassengerInteractionHandler.OnGrabPassenger += PlayerGrabPassengerInteractionHandler_OnGrabPassenger;
        GrabbedPassengerDropArea.OnPassengerDropped += GrabbedPassengerDropArea_OnPassengerDropped;
    }

    private void GrabbedPassengerDropArea_OnPassengerDropped(Passenger obj)
    {
        for (int i = 0; i < seatSlots.Length; i++)
        {
            seatSlots[i].grabbedPassengerDropArea.DisableInteractionCollider();
        }
    }

    private void PlayerGrabPassengerInteractionHandler_OnGrabPassenger(Passenger obj)
    {
        for (int i = 0; i < seatSlots.Length; i++)
        {
            if (seatSlots[i].slotStatus == SeatObjectSlot.SeatSlotStatus.EMPTY)
            {
                seatSlots[i].grabbedPassengerDropArea.EnableInteractionsCollider();
            }
        }
        //print("PlayerGrabPassengerInteractionHandler_OnGrabPassenger");
    }

    private void OnDisable()
    {
        PlayerGrabPassengerInteractionHandler.OnGrabPassenger -= PlayerGrabPassengerInteractionHandler_OnGrabPassenger;
        GrabbedPassengerDropArea.OnPassengerDropped -= GrabbedPassengerDropArea_OnPassengerDropped;
    }

    void Initialize()
    {
        entryRefrencePositionTransform.onCharacterReachLocation += OnReachSeatEntryPosition;

        for (int i = 0; i < seatSlots.Length; i++)
        {
            seatSlots[i].targetPosition.onCharacterReachLocation += OnCharacterReachSeatPosition;
        }
    }

    public bool isThereAnEmptySlotInSeat()
    {
        if (GetEmptySlot() != null)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Return Empty Slot
    /// </summary>
    /// <returns></returns>
    public SeatObjectSlot GetEmptySlot()
    {
        for (int i = 0; i < seatSlots.Length; i++)
        {
            if (seatSlots[i].slotStatus == SeatObjectSlot.SeatSlotStatus.EMPTY)
            {
                return seatSlots[i];
            }
        }

        return null;
    }

    public SeatObjectSlot GetSlot(GrabbedPassengerDropArea passengerDropArea)
    {
        for (int i = 0; i < seatSlots.Length; i++)
        {
            if (seatSlots[i].grabbedPassengerDropArea == passengerDropArea)
            {
                return seatSlots[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Rotate character to face available Slots, OnReachSeatEntryPosition complete OnCharacterReachSeatPosition
    /// </summary>
    /// <param name="character"></param>
    public void OnReachSeatEntryPosition(NPCharacter character)
    {
        character.transform.DORotate(entryRefrencePositionTransform.transform.eulerAngles, .5f)
        .OnComplete
        (
            () =>
            {
                MoveToSeatSlotPosition(character);
            }
         );
    }

    /// <summary>
    /// Sequence Tween of character Moving while rotating towards seating position
    /// </summary>
    /// <param name="character"></param>
    public void MoveToSeatSlotPosition(NPCharacter character)
    {
        emptySlot = GetEmptySlot();

        if (emptySlot == null)
        {
            Debug.LogError("Seat slot is null");
            return;
        }

        //print(character + "  Slot Euler: " + emptySlot.targetPosition.transform.eulerAngles);

        Sequence sequence = DOTween.Sequence();

        sequence
        .Append(character.transform.DOMove(emptySlot.targetPosition.transform.position, .5f))
        .Insert(0, character.transform.DORotate(emptySlot.targetPosition.transform.eulerAngles, sequence.Duration()))
        .OnComplete
        (
            () =>
            {
                emptySlot.targetPosition.onCharacterReachLocation?.Invoke(character);
            }
        );

    }


    /// <summary>
    /// Controls palyer interactions suring PayBusfare interactions
    /// </summary>
    /// <param name="v"></param>
    public void ToggleEnablePassengerColliders(bool v)
    {
        for (int i = 0; i < seatSlots.Length; i++)
        {
            if (seatSlots[i].passenger != null)
            {
                seatSlots[i].passenger.GetCharacter().ToggleNPCInteraction(v);
            }
        }
    }


    /// <summary>
    /// Rotate to match slot
    /// </summary>
    /// <param name="character"></param>
    public void OnCharacterReachSeatPosition(NPCharacter character)
    {
        character.SeatCharacter(this, emptySlot);
    }

    public MoodState GetCurrentMoodStateOfSeat()
    {
        for (int i = 0; i < seatSlots.Length; i++)
        {
            if (seatSlots[i].slotStatus == SeatObjectSlot.SeatSlotStatus.OCCUPIED)
            {
                return seatSlots[i].passenger.GetComponent<MoodHandler>().currentMood;
            }
        }
        return MoodState.Calm;
    }

    public List<MoodHandler> GetAllMoodHandlersInSeat()
    {
        List<MoodHandler> passengerMoods = new List<MoodHandler>();
        for (int i = 0; i < seatSlots.Length; i++)
        {
            if (seatSlots[i].slotStatus == SeatObjectSlot.SeatSlotStatus.OCCUPIED)
            {
                passengerMoods.Add(seatSlots[i].passenger.GetComponent<MoodHandler>());
            }
        }
        return passengerMoods;
    }

    public void UpdatePassengerMood()
    {

        if (seatSlots.Length < 2)
        {
            return;
        }

        if (isThereAnEmptySlotInSeat())
        {
            return;
        }



        var a = seatSlots[0].passenger.GetComponent<MoodHandler>();
        var b = seatSlots[1].passenger.GetComponent<MoodHandler>();

        //print($"{seatSlots[0].passenger} sitting with {seatSlots[1].passenger}");

        // Look up influence from matrix in Mood system manager
        float deltaA = MoodSystemManager.Instance.GetMatrixInfluence(a.currentMood, b.currentMood) * MoodSystemManager.Instance.influenceStrength;
        float deltaB = MoodSystemManager.Instance.GetMatrixInfluence(b.currentMood, a.currentMood) * MoodSystemManager.Instance.influenceStrength;

        // Apply and clamp
        a.moodValue = Mathf.Clamp(a.moodValue + deltaA, -100f, 100f);
        b.moodValue = Mathf.Clamp(b.moodValue + deltaB, -100f, 100f);
        //print($"Applying Mood Influence: {a.currentMood} ({deltaA}) -> {b.currentMood} | New Mood Value: {a.moodValue}");
        //print($"Applying Mood Influence: {b.currentMood} ({deltaB}) -> {a.currentMood} | New Mood Value: {b.moodValue}");

    }
}

    [System.Serializable]
    public class SeatObjectSlot
    {
        public enum SeatSlotStatus { EMPTY = 0, OCCUPIED = 1 }
        public SeatSlotStatus slotStatus;
        [Space]
        public MoveToTargetTransform targetPosition;
        [Space]
        public SeatObject owningSeat;
        [Space]
        public Passenger passenger;
        [Space]
        public GrabbedPassengerDropArea grabbedPassengerDropArea;
    }