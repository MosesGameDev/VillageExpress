using DG.Tweening;
using System;
using UnityEngine;


public class PlayerInteractionsHandler : MonoBehaviour
{
    [SerializeField] private bool canGrabPassenger;
    [SerializeField] private string debugText;
    [SerializeField] private bool isEnabled = true;
    public static PlayerInteractionsHandler instance;

    [Space]
    public NPCharacter dialogueCharacter;

    public Camera _camera;

    /// <summary>
    /// How far Items are for the player to interact with
    /// </summary>
    [SerializeField] private float interactibleDistance = 2;

    private IInteractible iInteractibleIntaface;
    public Interactible interactible;
    public static event Action<Interactible> OnPlayerStartInteraction;
    public static event Action<Interactible> OnInteractibleHighlighted;
    public static event Action<Interactible> OnInteractibleClearHighlight;

    public static event Action<Passenger> OnCheckPassengerGrab;

    /// <summary>
    /// Set to true when Item has been highlighted
    /// </summary>
    bool canInteract;

    bool castRay = true;

    bool InteractionStarted = false;


    private void Awake()
    {
        instance = this;
    }

    void OnConversationStart(Transform actor)
    {
        interactible = null;
        isEnabled = false;
        NullHit();

    }

    void OnConversationEnd(Transform actor)
    {
        isEnabled = true;
    }


    private void Update()
    {

        if (!isEnabled)
        {
            debugText = "Disabled";
            return;
        }

        if (InteractionStarted)
        {
            return;
        }


        DrawInteractionRayCast();
        if (interactible != null)
        {
            if (Input.GetKeyDown(interactible.interactionInput))
            {
                OnInteractButtonPressed();
                InteractionStarted = true;
            }
        }
    }


    void OnInteractButtonPressed()
    {
        OnPlayerStartInteraction?.Invoke(interactible);

        if (iInteractibleIntaface == null)
        {
            return;
        }

        if (!canInteract)
        {
            return;
        }

        iInteractibleIntaface.StartInteraction();
        iInteractibleIntaface.DisableHighlight();

        interactible.OnEndInteraction += RenableInteractions;
    }



    /// <summary>
    /// sets Interactible Object if raycast hit gameObject with IInteractible Interface
    /// </summary>
    private void DrawInteractionRayCast()
    {
        if (castRay)
        {
            Vector3 origin = new Vector3(_camera.transform.position.x, _camera.transform.position.y, _camera.transform.position.z);
            Vector3 direction = _camera.transform.forward;

            RaycastHit hit;
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

            if (iInteractibleIntaface != null)
            {
                iInteractibleIntaface.DisableHighlight();
                iInteractibleIntaface = null;
            }


            if (Physics.Raycast(ray, out hit, interactibleDistance))
            {
                Collider objectHit = hit.collider;
                Debug.DrawRay(origin, direction * interactibleDistance, Color.green);

                debugText = hit.collider.name;

                ///for skined mesh renders;
                if (objectHit.GetComponent<NPC_Collision>())
                {
                    if (!objectHit.GetComponent<NPC_Collision>().enabled)
                    {
                        return;
                    }

                    OnRayCastHitNPCCollider(objectHit.GetComponent<NPC_Collision>());
                    return;
                }

                ///Dont proceed if hit object does not contain Interactible Component
                if (!objectHit.GetComponent<Interactible>())
                {
                    if (canInteract)
                    {
                        canInteract = false;
                        interactible = objectHit.GetComponent<Interactible>();
                        OnInteractibleClearHighlight?.Invoke(interactible);
                    }

                    if (iInteractibleIntaface != null)
                    {
                        iInteractibleIntaface.DisableHighlight();
                        iInteractibleIntaface = null;
                    }
                    return;
                }

                OnRayCastHitInteractible(objectHit);

            }
            else
            {
                Debug.DrawRay(origin, direction * interactibleDistance, Color.red);

                NullHit();
            }
        }
    }

    private void NullHit()
    {
        if (interactible != null)
        {
            OnInteractibleClearHighlight?.Invoke(interactible);
            interactible = null;
        }

        if (iInteractibleIntaface != null)
        {
            iInteractibleIntaface.DisableHighlight();
            iInteractibleIntaface = null;
            canInteract = false;
        }

        canGrabPassenger = false;

    }


    void OnRayCastHitInteractible(Collider objectHit)
    {
        canInteract = true;
        interactible = objectHit.GetComponent<Interactible>();
        OnInteractibleHighlighted?.Invoke(interactible); ///Interaction instruction UI is subscribed, and is shown on Invoke Action i.e press 'E' to interact

        iInteractibleIntaface = objectHit.GetComponent<IInteractible>();
        iInteractibleIntaface.Highlight();
    }


    void OnRayCastHitNPCCollider(NPC_Collision collision)
    {
        if (!collision)
        {
            Debug.LogError("collision");
            return;
        }

        if (!collision.GetInteractible())
        {
            Debug.LogError("GetInteractible");
            return;
        }

        if (!collision.GetInteractible().GetComponent<Passenger>())
        {
            Debug.LogError("Passenger");
            return;
        }


        Passenger passenger = collision.GetInteractible().GetComponent<Passenger>();
        NPCharacter nPCharacter = collision.GetInteractible().GetComponent<NPCharacter>();

        ///Grab interaction
        if (nPCharacter.state == NPC_States.NPC_STATES.SITTING)
        {
            if (collision.isPrimaryCollider)
            {
                canGrabPassenger = true;
                passenger.GetComponent<NPC_InteractionsHandler>().Highlight();
                OnCheckPassengerGrab?.Invoke(passenger);
            }
            else
            {
                canGrabPassenger = false;
                passenger.GetComponent<NPC_InteractionsHandler>().DisableHighlight();
                OnCheckPassengerGrab?.Invoke(null);
            }
        }


        ///Pay busfareinteraction
        ///Bus passenger can only interact when waiting for interaction, or when sitting in a moving vehicle and fare is pending
        if (passenger.passengerStatus == PassengerStatusEnum.PassengerStatus.WAITING_FOR_INTERACTION
        || (nPCharacter.state == NPC_States.NPC_STATES.SITTING && SceneRegistry.Instance.vE_Vehicle.status == VE_StatusEnum.vE_Status.MOVING
        && passenger.commute.fareStatus == PassengerCommute.PassengerCommuteEnum.PENDING))
        {
            
            canInteract = true;
            interactible = collision.GetInteractible();
            OnInteractibleHighlighted?.Invoke(interactible); ///Interaction instruction UI is subscribed, and is shown on Invoke Action i.e press 'E' to interact

            //nPCharacter.proceduralAnimController.EnableLookAnimator(SceneRegistry.Instance.player.lookAtTarget);

            iInteractibleIntaface = collision.GetInteractible().GetComponent<IInteractible>();
            iInteractibleIntaface.Highlight();
        }


    }


    /// <summary>
    /// Allow for raycast to Interactibles
    /// </summary>
    public void RenableInteractions(Interactible interactible)
    {

        float angle = 0;
        DOTween.To(() => angle, x => angle = x, 1, .5f)
            .OnComplete(() =>
            {
                if (InteractionStarted)
                {
                    InteractionStarted = false;
                    OnInteractibleHighlighted?.Invoke(interactible);
                }
            });
    }

}
