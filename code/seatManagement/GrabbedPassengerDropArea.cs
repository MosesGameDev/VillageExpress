using DG.Tweening;
using UnityEngine;

public class GrabbedPassengerDropArea : Interactible
{

    [Space]
    [SerializeField] private SeatObject seat;
    [SerializeField] private Transform arrowVisual;

    Interactible _interactible;

    Vector3 startPos;
    Vector3 initialScale;
    Passenger grabbedPassenger;

    public static event System.Action<Passenger> OnPassengerDropped;

    private void OnEnable()
    {
        PlayerInteractionsHandler.OnCheckPassengerGrab += PlayerInteractionsHandler_OnCheckPassengerGrab;
    }

    private void PlayerInteractionsHandler_OnCheckPassengerGrab(Passenger _passenger)
    {
        if (_passenger != null)
        {
            grabbedPassenger = _passenger;
        }
    }


    private void Awake()
    {
        OnStartInteraction += OnInteractionStart;
        OnEndInteraction += OnInteractionEnd;
        onHighlight += OnHighlight;
        onDisableHighlight += OnDisableHighlight;
    }

    private void OnDisable()
    {
        OnStartInteraction -= OnInteractionStart;
        OnEndInteraction -= OnInteractionEnd;
        onHighlight -= OnHighlight;
        onDisableHighlight -= OnDisableHighlight;
        PlayerInteractionsHandler.OnCheckPassengerGrab -= PlayerInteractionsHandler_OnCheckPassengerGrab;
    }

    private void Start()
    {
        initialScale = arrowVisual.localScale;
        startPos = arrowVisual.transform.localPosition;
        seat = GetComponentInParent<SeatObject>();
    }


    public void OnInteractionStart(Interactible interactible)
    {
        _interactible = interactible;
        DropPassenger();
    }

    public void DropPassenger()
    {
        if (grabbedPassenger == null)
        {
            return;
        }

        SeatObjectSlot slot = seat.GetSlot(this);

        grabbedPassenger.transform.SetParent(null);
        Vector3 seatPos = slot.targetPosition.transform.position;
        seatPos.y += .5f;

        Sequence dropPassengerSequence = DOTween.Sequence();

        dropPassengerSequence.OnStart(() => { grabbedPassenger.transform.SetParent(null); });

        dropPassengerSequence
        .Append(grabbedPassenger.transform.DOMove(seatPos, 0.5f))
        .Join(grabbedPassenger.transform.DORotate(slot.targetPosition.transform.eulerAngles, 0.5f))
        .Append(grabbedPassenger.transform.DOMoveY((seatPos.y -= .5f), 0.5f).SetEase(Ease.OutBounce));

        dropPassengerSequence.OnComplete(() => OnDropPassengers(slot));
    }

    void OnDropPassengers(SeatObjectSlot slot)
    {
        grabbedPassenger.transform.SetParent(seat.transform);
        grabbedPassenger.GetComponent<NPCharacter>().ToggleCharacterColliders(true);

        grabbedPassenger.GetComponent<NPCharacter>().proceduralAnimController.DisableBoneSimulators();
        grabbedPassenger.GetComponent<NPCharacter>().proceduralAnimController.DisableLookAnimator();

        slot.passenger = grabbedPassenger;
        slot.slotStatus = SeatObjectSlot.SeatSlotStatus.OCCUPIED;

        grabbedPassenger.transform.position = slot.targetPosition.transform.position;
        grabbedPassenger.transform.eulerAngles = slot.targetPosition.transform.eulerAngles;

        OnPassengerDropped?.Invoke(grabbedPassenger);
        OnEndInteraction.Invoke(_interactible);
        grabbedPassenger = null;
        print("Passenger Dropped");
    }

    void OnInteractionEnd(Interactible interactible)
    {
        PlayerInteractionsHandler.instance.RenableInteractions(interactible);
    }


    public void EnableInteractionsCollider()
    {
        arrowVisual.DOKill();
        arrowVisual.transform.localPosition = startPos;
        arrowVisual.gameObject.SetActive(true);
        arrowVisual.DOMoveY(arrowVisual.transform.position.y - 0.3f, 1).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.OutBounce);
        GetComponent<Collider>().enabled = true;
    }

    public void DisableInteractionCollider()
    {
        arrowVisual.localScale = initialScale;
        arrowVisual.transform.localPosition = startPos;
        GetComponent<Collider>().enabled = false;
        arrowVisual.gameObject.SetActive(false);

    }


    void OnHighlight()
    {
        arrowVisual.transform.DOScale(initialScale * 1.5f, 0.5f).SetEase(Ease.OutBack);
    }


    void OnDisableHighlight()
    {
        arrowVisual.transform.DOScale(initialScale, 0.5f).SetEase(Ease.OutBack);
    }

}
