using System;
using UnityEngine;

public class FoldingDoorsInteractionHandler : Interactible
{
    [SerializeField] private Animator doorAnimator;

    public Action<bool> onDoorOpen;

    public bool isOpen = false;
    Interactible _interactible;

    private void Start()
    {
        OnStartInteraction += PressButton;
        OnEndInteraction += OnInteractionEnd;

    }

    public void PressButton(Interactible interactible)
    {
        _interactible = interactible;

        if (!isOpen)
        {
            OpenDoor();
            return;
        }

        CloseDoor();
    }

    void OnInteractionEnd(Interactible interactible)
    {
        PlayerInteractionsHandler.instance.RenableInteractions(interactible);
    }

    public void OpenDoor()
    {
        if (!isOpen)
        {
            doorAnimator.SetBool("isOpen", true);
        }
    }

    public void CloseDoor()
    {
        if (isOpen)
        {
            doorAnimator.SetBool("isOpen", false);
        }
    }


    /// <summary>
    /// Called by animator event
    /// </summary>
    public void OnDoorOpen()
    {
        isOpen = true;
        OnEndInteraction.Invoke(_interactible);
        onDoorOpen?.Invoke(isOpen);
    }

    /// <summary>
    /// Called by animator event
    /// </summary>
    public void OnDoorClose()
    {
        isOpen = false;
        OnEndInteraction.Invoke(_interactible);
        onDoorOpen?.Invoke(isOpen);
    }
}
