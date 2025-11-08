using UnityEngine;

public class PassengerBusInteraction : Interaction
{
    Passenger passenger;
    FirstPersonController firstPersonController;
    NPC_InteractionsHandler _InteractionsHandler;


    private void Start()
    {
        if(!SceneRegistry.Instance)
        {
            return;
        }

        if (transform.parent.GetComponent<Passenger>())
        {
            passenger = transform.parent.GetComponent<Passenger>();
            _InteractionsHandler = passenger.GetComponent<NPC_InteractionsHandler>();
            firstPersonController = SceneRegistry.Instance.player.firstPersonController;
        }
        else
        {
            Debug.LogError($"{this} requires parent transfom ro have type 'Passenger' attached ");
        }
    }

    public override void PlayInteraction()
    {
        StartBusInteraction();
    }

    void StartBusInteraction()
    {
        passenger.PayBusFare();
        EndInteraction();
    }

    void EndInteraction()
    {
        _InteractionsHandler.EndNPCInteraction(passenger.GetComponent<NPC_InteractionsHandler>());
    }

}
