using DG.Tweening;
using FIMSpace.FProceduralAnimation;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.AI;
using static NPC_States;


[RequireComponent(typeof(NPC_MovementController))]

public class NPCharacter : MonoBehaviour
{
    public NPC_STATES state; // Current Character State, States show current character Action

    [Space]
    public string characterName; //Character Name Id

    [Title("Refrences")]
    public SeatObjectSlot vehicleSeatingSlot; //Seating slot assigned to character
    NPC_MovementController movementController; //Controls Character Movement and Animations
    public NPC_ProceduralAnimController proceduralAnimController;

    [Title("Colliders")]
    [SerializeField] private Collider[] colliders;


    public static event Action<SeatObject, SeatObjectSlot, NPCharacter> OnCharacterSeated;


    private void Start()
    {
        movementController = GetComponent<NPC_MovementController>();
        GetComponent<Passenger>().commute.passenger = GetComponent<Passenger>();
    }


    private void OnEnable()
    {
        DialogueEvent_SO.PassengerBoardVehicleEvent += DialogueEvent_SO_PassengerBoardVehicleEvent1;
    }

    private void DialogueEvent_SO_PassengerBoardVehicleEvent1(string id)
    {
        if (id == characterName)
        {
            proceduralAnimController.DisableBoneSimulators();
            proceduralAnimController.DisableLookAnimator(true);

            Passenger passenger = GetComponent<Passenger>();
            SceneRegistry.Instance.vE_Vehicle.passengerCrowdBoardingHelper.AddPassenger(passenger);
        }
    }


    private void OnDisable()
    {
        DialogueEvent_SO.PassengerBoardVehicleEvent -= DialogueEvent_SO_PassengerBoardVehicleEvent1;
    }


    public NPC_MovementController GetMovementController()
    {
        if (movementController == null)
        {
            movementController = GetComponent<NPC_MovementController>();
        }

        return movementController;
    }


    public void DisableAIAgent()
    {
        movementController.GetNavAgent().enabled = false;
    }


    public void EnableAgent()
    {
        movementController.GetNavAgent().enabled = true;
    }


    /// <summary>
    /// Enters Character into Sitting State; Animator "isSitting=true"
    /// </summary>
    /// <param name="_seatSlot"></param>
    public void SeatCharacter(SeatObject _seat, SeatObjectSlot _seatSlot)
    {
        state = NPC_STATES.SITTING;
        GetComponent<Passenger>().passengerStatus = PassengerStatusEnum.PassengerStatus.IN_TRANSIT;
        GetComponent<LegsAnimator>().enabled = false;

        if(GetComponent<NavMeshAgent>().enabled)
        {
            GetComponent<NavMeshAgent>().enabled = false;
        }

        vehicleSeatingSlot = _seatSlot;
        vehicleSeatingSlot.owningSeat = _seat;

        transform.SetParent(_seat.transform);
        _seatSlot.slotStatus = SeatObjectSlot.SeatSlotStatus.OCCUPIED;

        Passenger passenger = GetComponent<Passenger>();
        _seatSlot.passenger = passenger;

        if (!SceneRegistry.Instance.vE_Vehicle.passengers.Contains(passenger))
        {
            SceneRegistry.Instance.vE_Vehicle.passengers.Add(passenger);
        }

        movementController.GetCharacterAnimator().SetBool("isSitting", true);
        OnCharacterSeated?.Invoke(_seat, _seatSlot, this);
    }


    /// <summary>
    /// Tween moves character to seat exit, 
    /// move tween happens after standup anim plays
    /// </summary>
    /// <param name="doTweenExitSeatSeqence"></param>
    public void ExitSeat(out Sequence doTweenExitSeatSeqence, float delay = .2f)
    {
        if (state != NPC_STATES.SITTING)
        {
            doTweenExitSeatSeqence = null;
            return;
        }

        transform.SetParent(null);

        movementController.GetCharacterAnimator().SetBool("isSitting", false);
        float animClipLength = movementController.GetCharacterAnimator().GetCurrentAnimatorClipInfo(0)[0].clip.length;

        Vector3 seatExitPosition = vehicleSeatingSlot.owningSeat.entryRefrencePositionTransform.transform.position;

        Vector3 lookAt = seatExitPosition;
        lookAt.y = transform.position.y;

        Sequence sequence = DOTween.Sequence();
        doTweenExitSeatSeqence = sequence;


        sequence
        .OnStart
        (
            () =>
            {
                GetComponent<LegsAnimator>().enabled = true;
            }
        )
        .Append(transform.DOMove(seatExitPosition, 1).SetDelay(animClipLength + delay))
        .Insert(0, transform.DOLookAt(lookAt, sequence.Duration()))
        .OnComplete
        (
            () =>
            {
                vehicleSeatingSlot.slotStatus = SeatObjectSlot.SeatSlotStatus.EMPTY;
                vehicleSeatingSlot.passenger = null;
                vehicleSeatingSlot = null;
            }
        );


    }


    public void ToggleCharacterColliders(bool v)
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = v;
        }
    }

    public void ToggleNPCInteraction(bool v)
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].GetComponent<NPC_Collision>().enabled = v;
        }
    }


    public void DisableCharacter()
    {
        GetComponent<LegsAnimator>().enabled = false;
        DisableAIAgent();
        state = NPC_STATES.IDLE;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Added a delay when enabling the character, called on spawn
    /// delay gives AIAgent time to initialize
    /// </summary>
    public void EnableCharacter()
    {
        Invoke("InitializeCharacter", .5f);
    }

    void InitializeCharacter()
    {
        GetComponent<LegsAnimator>().enabled = true;
        EnableAgent();
        state = NPC_STATES.IDLE;
        gameObject.SetActive(true);
    }

}
