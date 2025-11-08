using System;

public class PickUpItemInteractionHandler : Interactible
{
    Interactible _interactible;

    public Action onPickupItem;

    void Start()
    {
        OnStartInteraction += PickUpItem;
        OnEndInteraction += OnInteractionEnd;
    }

    void PickUpItem(Interactible interactible)
    {
        _interactible = interactible;
        gameObject.SetActive(false);

        OnPickupBanKNote();
    }

    void OnPickupBanKNote()
    {
        onPickupItem.Invoke();
        OnInteractionEnd(_interactible);
    }


    void OnInteractionEnd(Interactible interactible)
    {
        PlayerInteractionsHandler.instance.RenableInteractions(interactible);
    }

}
