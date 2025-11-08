using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;

public class PlayerGrabPassengerInteractionHandler : MonoBehaviour
{
    [SerializeField] private Transform grabPositionTransform;
    [SerializeField] private float grabDuration = 0.5f;
    [SerializeField] private float grabmMouseSensitivity = 0.5f;

    [ReadOnly]
    [SerializeField] private Passenger grabbedPassenger;
    [SerializeField] private FirstPersonController playerController;

    public static event Action<Passenger> OnGrabPassenger;

    [Space]
    public KeyCode grabInput = KeyCode.G;


    private void OnEnable()
    {
        PlayerInteractionsHandler.OnCheckPassengerGrab += PlayerInteractionsHandler_OnCheckPassengerGrab;
    }

    private void PlayerInteractionsHandler_OnCheckPassengerGrab(Passenger _passenger)
    {
        grabbedPassenger = _passenger;
    }

    private void OnDisable()
    {
        PlayerInteractionsHandler.OnCheckPassengerGrab -= PlayerInteractionsHandler_OnCheckPassengerGrab;
    }

    private void Start()
    {
        grabmMouseSensitivity = playerController.mouseSensitivity;
    }


    private void Update()
    {
        if (Input.GetKeyDown(grabInput))
        {
            if (grabbedPassenger != null)
            {
                GrabPassenger();
            }
        }
    }

    [Button]
    public void GrabPassenger()
    {
        if (grabbedPassenger == null)
        {
            return;
        }
        grabbedPassenger.GetComponent<NPCharacter>().ToggleCharacterColliders(false);

        Sequence sequence = DOTween.Sequence();


        sequence
            .Append(grabbedPassenger.transform.DOMove(grabPositionTransform.position, grabDuration))
            .Join(grabbedPassenger.transform.DORotate(grabPositionTransform.eulerAngles, sequence.Duration()));

        sequence.OnComplete(() => { OnPassengerGrabbed(); });

        grabbedPassenger.transform.parent = grabPositionTransform;
        
    }


    private void OnPassengerGrabbed()
    {
        grabbedPassenger.transform.parent = grabPositionTransform;
        grabbedPassenger.GetComponent<NPCharacter>().proceduralAnimController.EnableLookAnimator();
        grabbedPassenger.GetComponent<NPCharacter>().proceduralAnimController.EnableBoneSimulators();

        grabbedPassenger.GetComponent<NPCharacter>().vehicleSeatingSlot.slotStatus = SeatObjectSlot.SeatSlotStatus.EMPTY;
        grabbedPassenger.GetComponent<NPCharacter>().vehicleSeatingSlot.passenger = null;

        
        OnGrabPassenger?.Invoke(grabbedPassenger);
        grabbedPassenger = null;
    }
}
