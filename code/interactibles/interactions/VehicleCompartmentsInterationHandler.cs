using DG.Tweening;
using UnityEngine;

public class VehicleCompartmentsInterationHandler : Interactible
{
    [SerializeField] private Transform CompartmentTransform;
    [Space]
    [SerializeField] private Vector3 openRot;
    [SerializeField] private Vector3 closeRot;
    [Space]
    [SerializeField] private Ease openEase;
    [SerializeField] private Ease closeEase;
    [Space]
    [SerializeField] private float tweenDuration = 1f;

    bool isOpen = false;
    Interactible _interactible;


    private void Start()
    {
        OnStartInteraction += OnButtonPress;
        OnEndInteraction += OnInteractionEnd;
    }

    private void OnButtonPress(Interactible interactible)
    {
        _interactible = interactible;

        if (!isOpen)
        {
            OpenCompartment();
            return;
        }

        CloseCompartment();

    }

    void OpenCompartment()
    {
        CompartmentTransform.DOLocalRotate(openRot, tweenDuration)
        .SetEase(openEase)
        .OnComplete
        (
            () =>
            {
                isOpen = true;
                OnInteractionEnd(_interactible);
            }
        );
    }

    void CloseCompartment()
    {
        CompartmentTransform.DOLocalRotate(closeRot, tweenDuration / 2)
        .SetEase(closeEase)
        .OnComplete
        (
            () =>
            {
                isOpen = false;
                OnInteractionEnd(_interactible);
            }
        );
    }

    /// <summary>
    /// Called once open/close tween is Completed
    /// </summary>
    /// <param name="interactible"></param>
    void OnInteractionEnd(Interactible interactible)
    {
        PlayerInteractionsHandler.instance.RenableInteractions(interactible);
    }

}
