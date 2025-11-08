using System;
using UnityEngine;

/// <summary>
/// Plays interactions with player i.e dialogue and vehicle boarding actions
/// Think of this as an 'interactions' player for class Interaction
/// </summary>
public class NPC_InteractionsHandler : Interactible
{
    public bool isEnabled = true;
    [Space]
    [SerializeField] private Interaction quickTimeInteraction;
    [SerializeField] private Interaction busPassengerInteraction;
    [SerializeField] private NPCEncounterTracker conversationTracker;

    Passenger passenger;

    //public static event Action <Passenger> onInteractionStarted;
    //public static event Action <Passenger> onInteractionEnded;

    private void Start()
    {
        OnStartInteraction += StartNPCInteraction;
        passenger = GetComponent<Passenger>();
    }

    void StartNPCInteraction(Interactible interactible)
    {
        if (!isEnabled) { return; }

        if (interactible = this)
        {

            if (passenger.passengerStatus == PassengerStatusEnum.PassengerStatus.IN_TRANSIT)
            {
                busPassengerInteraction.PlayInteraction();
                return;
            }


            quickTimeInteraction.PlayInteraction();
        }
    }


    private void OnConversationEnd(Transform t)
    {
        EndNPCInteraction(this);
    }


    public void Highlight()
    {
        if (!isEnabled) { return; }

        ToggleOutline(true);
    }


    public void DisableHighlight()
    {
        if (!isEnabled) { return; }

        ToggleOutline(false);
    }


    public void EndNPCInteraction(Interactible interactible)
    {
        PlayerInteractionsHandler.instance.RenableInteractions(interactible);
    }
}
