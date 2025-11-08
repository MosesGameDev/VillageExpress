using DG.Tweening;
using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;

public class PlayerSeatInteraction : Interactible
{
    [Space]
    [SerializeField] private int seatSlotIndex;
    [SerializeField] private SeatObject seat;

    [Space]
    public Collider[] triggerColliders;

    Interactible _interactible;
    VE_StatusEnum.vE_Status vehicleStatus;

    bool isPlayerInSeat = false;

    public static event Action<PlayerSeatInteraction> OnPlayerMoveToSeat;
    public static event Action<List<MoodHandler>> OnPlayerLookAtSeat;
    private void Awake()
    {
        OnStartInteraction += OnInteractionStart;
        OnEndInteraction += OnInteractionEnd;
        onHighlight += OnHighlight;
        onDisableHighlight += OnDisableHighlight;
    }

    private void OnEnable()
    {
        VE_Vehicle.OnVehicleStatusChanged += VE_Vehicle_OnVehicleStatusChanged;
    }

    private void VE_Vehicle_OnVehicleStatusChanged(VE_StatusEnum.vE_Status status)
    {
        switch (status)
        {
            case VE_StatusEnum.vE_Status.MOVING:
                ToggleTriggerColliders(true);
                break;
            case VE_StatusEnum.vE_Status.STOPPED:
                ToggleTriggerColliders(false);
                break;
        }
    }

    private void OnDisable()
    {
        OnStartInteraction -= OnInteractionStart;
        OnEndInteraction -= OnInteractionEnd;
        onHighlight -= OnHighlight;
        onDisableHighlight -= OnDisableHighlight;
        VE_Vehicle.OnVehicleStatusChanged -= VE_Vehicle_OnVehicleStatusChanged;

    }



    public void OnInteractionStart(Interactible interactible)
    {
        if (seat.seatSlots[seatSlotIndex].slotStatus == SeatObjectSlot.SeatSlotStatus.OCCUPIED)
        {
            return;
        }

        if (vehicleStatus == VE_StatusEnum.vE_Status.STOPPED)
        {
            OnEndInteraction.Invoke(_interactible);
            return;
        }

        _interactible = interactible;


        MoveToSeat();
    }


    void OnHighlight()
    {
        foreach (SeatObjectSlot slot in seat.seatSlots)
        {
            if (slot.slotStatus == SeatObjectSlot.SeatSlotStatus.OCCUPIED)
            {
                slot.passenger.GetComponent<NPC_InteractionsHandler>().Highlight();
            }
        }

        OnPlayerLookAtSeat?.Invoke(seat.GetAllMoodHandlersInSeat());
    }


    void OnDisableHighlight()
    {
        foreach (SeatObjectSlot slot in seat.seatSlots)
        {
            if (slot.slotStatus == SeatObjectSlot.SeatSlotStatus.OCCUPIED)
            {
                slot.passenger.GetComponent<NPC_InteractionsHandler>().DisableHighlight();
            }
        }
    }


    public void ToggleTriggerColliders(bool state)
    {
        foreach (Collider col in triggerColliders)
        {
            col.enabled = state;
        }
    }


    Vector3 initialWorldPosition;
    float initialLookSensitivity;
    private void MoveToSeat()
    {
        if (SceneRegistry.Instance == null)
        {
            Debug.LogError("Missing vehicle refrence");
            return;
        }



        SceneRegistry.Instance.vE_Vehicle.cachedPlayerPositionRefrence.position = SceneRegistry.Instance.player.transform.position;
        initialWorldPosition = SceneRegistry.Instance.player.transform.position;

        Transform playerTransform = SceneRegistry.Instance.player.transform;

        playerTransform.parent = seat.transform;

        SceneRegistry.Instance.player.firstPersonController.DisableMovement();
        SceneRegistry.Instance.player.firstPersonController.cameraCanMove = false;


        // Convert the world position to local space relative to the seat
        Vector3 targetLocalPosition = seat.transform.InverseTransformPoint(transform.position);
        initialLookSensitivity = SceneRegistry.Instance.player.firstPersonController.mouseSensitivity;
        SceneRegistry.Instance.player.firstPersonController.mouseSensitivity = .25f;
        Sequence sequence = DOTween.Sequence();
        sequence
        .Append(playerTransform.DOLocalMove(targetLocalPosition, 1))
        .Join(playerTransform.DORotate(seat.seatSlots[0].targetPosition.transform.eulerAngles, sequence.Duration()))
        .Join(SceneRegistry.Instance.player.firstPersonController.playerCamera.transform.DOLocalRotate(Vector3.zero, sequence.Duration()/2))
        .OnComplete(OnMoveToSeat);
    }


    void OnMoveToSeat()
    {
        if (!SceneRegistry.Instance.player.transform.parent)
        {
            SceneRegistry.Instance.player.transform.SetParent(SceneRegistry.Instance.vE_Vehicle.transform);
        }

        isPlayerInSeat = true;

        SceneRegistry.Instance.player.firstPersonController.cameraCanMove = true;
        OnPlayerMoveToSeat?.Invoke(this);
        //OnEndInteraction.Invoke(_interactible);
    }

    private void Update()
    {
        if (isPlayerInSeat && Input.GetKeyDown(interactionInput))
        {
            ExitSeat();
        }
    }

    public void ExitSeat()
    {
        isPlayerInSeat = false;

        Transform playerTransform = SceneRegistry.Instance.player.transform;
        playerTransform.SetParent(SceneRegistry.Instance.vE_Vehicle.transform);

        Vector3 targetP = SceneRegistry.Instance.vE_Vehicle.cachedPlayerPositionRefrence.position;


        playerTransform.position = new Vector3(targetP.x, initialWorldPosition.y, targetP.z);
        print("Player Exit Position: " + new Vector3(targetP.x, initialWorldPosition.y, targetP.z));

        SceneRegistry.Instance.player.firstPersonController.mouseSensitivity = initialLookSensitivity;
        SceneRegistry.Instance.player.firstPersonController.EnableMovement();
        OnEndInteraction.Invoke(_interactible);

    }

    void OnInteractionEnd(Interactible interactible)
    {
        PlayerInteractionsHandler.instance.RenableInteractions(interactible);
    }

}
