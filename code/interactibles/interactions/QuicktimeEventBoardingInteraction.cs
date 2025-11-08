using UnityEngine;

/// <summary>
/// This interaction plays if when the player is picking up passengers from a Bus stop
/// Player approach NPC character and presses interaction button to start interaction
/// Plays QTE; if success NPC boards VEH
/// </summary>
public class QuicktimeEventBoardingInteraction : Interaction
{
    QuickTimeInteractionUI quickTimeInteractionUI;
    NPCharacter owningCharacter;
    bool isRunning;

    NPC_InteractionsHandler interactionHandler;

    private void Start()
    {
        if (!SceneRegistry.Instance)
        {
            return;
        }
        if (transform.parent.GetComponent<NPCharacter>())
        {
            owningCharacter = transform.parent.GetComponent<NPCharacter>();
            interactionHandler = owningCharacter.GetComponent<NPC_InteractionsHandler>();
            quickTimeInteractionUI = FindFirstObjectByType<QuickTimeInteractionUI>();
            quickTimeInteractionUI.OnStop += OnStopQTE;
        }
        else
        {
            Debug.LogError($"{this} requires parent transfom ro have type 'NPCharacter' attached ");
        }

    }

    public override void PlayInteraction()
    {
        if (quickTimeInteractionUI == null)
        {
            return;
        }

        isRunning = true;
        quickTimeInteractionUI.PlayQuickTimeInteraction(owningCharacter.GetComponent<Passenger>());
    }

    private void Update()
    {
        if (isRunning)
        {
            if (Input.GetKeyDown(InteractionKeyCode))
            {
                isRunning = false;
                quickTimeInteractionUI.StopQuicktime();
            }
        }
    }

    void OnStopQTE(bool b)
    {
        if (b)
        {
            OnSuccessStopQTEInZone();
            return;
        }

        OnFailedStopQTEInZone();
    }

    /// <summary>
    /// Player stops moving QTE arrow within the required range
    /// </summary>
    private void OnSuccessStopQTEInZone()
    {
        CompleteInteraction();
    }


    /// <summary>
    /// Player FAILS to stop moving QTE arrow within the required range
    /// </summary>
    private void OnFailedStopQTEInZone()
    {
        CompleteInteraction();
    }

    private void CompleteInteraction()
    {
        var passenger = owningCharacter.GetComponent<Passenger>();

        //PRINT OWNING CHARACTER AND INTERACTIBLE MISMATCH
        if (owningCharacter == null)
        {
            Debug.LogError($"{this} owningCharacter {owningCharacter.name} has no Passenger component");
            return;
        }

        //Filter interactions Handlin to only affect the character in focus
        if (PlayerInteractionsHandler.instance.interactible != owningCharacter.GetComponent<NPC_InteractionsHandler>())
        {
            print("<color=red><b>MISMATCH: </b></color> " + PlayerInteractionsHandler.instance.interactible.name + " - " + owningCharacter.GetComponent<NPC_InteractionsHandler>().name);
            return;
        }

        //passenger.passengerStatus = PassengerStatusEnum.PassengerStatus.REFUSED_BOARDING;
        owningCharacter.GetComponent<Passenger>().passengerStatus = PassengerStatusEnum.PassengerStatus.BOARDING_VEHICLE;
        interactionHandler.EndNPCInteraction(owningCharacter.GetComponent<NPC_InteractionsHandler>());
        DisableCharacterProceduralAnimations();

        VE_Vehicle vehicle = SceneRegistry.Instance.vE_Vehicle;
        vehicle.GetComponent<PassengerCrowdBoardingHelper>().AddPassenger(owningCharacter.GetComponent<Passenger>());

        //owningCharacter.GetMovementController().MoveToTargetLocation(passenger.fareDetails.pickup.GetRandomBusStopDestination());

    }


    void DisableCharacterProceduralAnimations()
    {
        owningCharacter.proceduralAnimController.DisableBoneSimulators();
        owningCharacter.proceduralAnimController.DisableLookAnimator(true);
    }

}
