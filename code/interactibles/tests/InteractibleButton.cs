using DG.Tweening;
using UnityEngine;

public class InteractibleButton : Interactible
{
    [Header("Button refrences")]
    [SerializeField] Transform buttonTransform;

    [Space]
    [SerializeField] float buttonPressedUpPositionY;
    [SerializeField] float buttonPressedDownPositionY;

    bool isPressedDown = false;


    private void Start()
    {
        OnStartInteraction += Interact;
        OnEndInteraction += OnInteractionEnd;
    }

    public void Interact(Interactible interactible)
    {
        if (!isPressedDown)
        {
            buttonTransform.DOLocalMoveY(buttonPressedDownPositionY, 0.2f).SetEase(Ease.OutBounce).OnComplete(() => { OnEndInteraction.Invoke(interactible); }); ;
            isPressedDown = true;
            return;
        }

        buttonTransform.DOLocalMoveY(buttonPressedUpPositionY, 0.2f).SetEase(Ease.OutBounce).OnComplete(() => { OnEndInteraction.Invoke(interactible); });
        isPressedDown = false;
    }

    void OnInteractionEnd(Interactible interactible)
    {
        PlayerInteractionsHandler.instance.RenableInteractions(interactible);
    }

}
