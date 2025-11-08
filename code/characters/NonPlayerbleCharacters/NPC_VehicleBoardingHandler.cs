using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

[RequireComponent(typeof(NPCharacter))]
[RequireComponent(typeof(Passenger))]


public class NPC_VehicleBoardingHandler : MonoBehaviour
{
    [Title("Movement")]
    [SerializeField] private float movementDuration = 2.25f;
    [SerializeField] private bool chooseRandomSeat = false;

    
    [Title("Baording Taunt Response")]
    [SerializeField] private float tauntResponseValue; //Fills up based on player taunts, passenger boards vehicle if value == 1
    [SerializeField] private float tauntResponseFillRate = 0.05f;

    VE_Vehicle villageExpress;
    NPC_MovementController nPC_MovementController;
    NPCharacter _character;
    Passenger _passenger;

    public static event Action<float, Passenger> OnTauntValueUpdated;

    private void Start()
    {
        if (!SceneRegistry.Instance)
        {
            return;
        }
        villageExpress = SceneRegistry.Instance.vE_Vehicle;
        nPC_MovementController = GetComponent<NPC_MovementController>();
        _character = GetComponent<NPCharacter>();
        _passenger = GetComponent<Passenger>();
        Initialize();
    }


    /// <summary>
    /// Initialize Action events
    /// </summary>
    void Initialize()
    {
        if(villageExpress != null)
        {
            villageExpress.entranceTargetTransform.onCharacterReachLocation += OnReachVehicleEntrance;
            villageExpress.isleTargetTransform.onCharacterReachLocation += OnBoardingCompleted;
        }
    }

    #region BoardingVehicle


    /// <summary>
    /// Moves character to the vehicles entrance to begin boarding Action
    /// </summary>
    [Button]
    public void EditorButtonBoardVehicle()
    {
        if (GetComponent<NPCharacter>().state == NPC_States.NPC_STATES.SITTING)
        {
            return;
        }

        if (nPC_MovementController == null)
        {
            print("<color=red><b> Unable to find 'NPC_MovementController': Make sure it is in the scene </b></color>");
            return;
        }

        if (villageExpress == null)
        {
            print("<color=red><b> Unable to find 'Village Express Vehicle': Make sure it is in the scene </b></color>");
            return;
        }


        nPC_MovementController.MoveToTargetLocation(villageExpress.entranceTargetTransform);
        _passenger.passengerStatus = PassengerStatusEnum.PassengerStatus.BOARDING_VEHICLE;

    }


    /// <summary>
    /// Player Taunts the character; character responds by boarding the vehicle if taunt value is 1
    /// This is represented in game by a fill bar
    /// </summary>
    public void HandleTauntResponse()
    {
        if (tauntResponseValue < 1 )
        {
            tauntResponseValue += tauntResponseFillRate;

            if (tauntResponseValue >= 1)
            {
                SceneRegistry.Instance.vE_Vehicle.passengerCrowdBoardingHelper.AddPassenger(_passenger);
                tauntResponseValue = 1;
                OnTauntValueUpdated?.Invoke(1, _passenger);
                return;
            }

            OnTauntValueUpdated?.Invoke(tauntResponseValue, _passenger);
        }
    }

    /// <summary>
    /// OnReachVehicleEntrance the character Tweens to get onto the vehicle; NavAgent is disabled
    /// </summary>
    /// <param name="character"></param>
    void OnReachVehicleEntrance(NPCharacter character)
    {
        if (_character != character)
        {
            return;
        }

        Vector3 moveToTargetPosition = villageExpress.isleTargetTransform.transform.position;

        _character.state = NPC_States.NPC_STATES.BOARDING_VEH;
        character.DisableAIAgent();

        Vector3 lookAtPosition = villageExpress.isleTargetTransform.transform.position;
        lookAtPosition.y = transform.position.y;

        character.GetMovementController().AnimatorSetSpeed(.1f);
        transform.DOLookAt(lookAtPosition, 0.5f);
        character.transform.DOMove(moveToTargetPosition, movementDuration).SetDelay(0.6f)
        .OnComplete
        (
            () =>
            {
                character.GetMovementController().AnimatorSetSpeed(0f);
                villageExpress.isleTargetTransform.onCharacterReachLocation?.Invoke(character);
            }
        );

    }

    /// <summary>
    /// Boarding Action completed the character; character can be assigned seat in vehicle
    /// </summary>
    /// <param name="character"></param>
    void OnBoardingCompleted(NPCharacter character)
    {
        SeatObjectSlot slot = null;
        SeatObject seat = null;
        SceneRegistry.Instance.vE_Vehicle.seatAssigner.AssignSeatToCharacter(character, chooseRandomSeat, out slot, out seat);
    }


    #endregion



    #region ExitingVehicle


    [Button("Exit Vehicle")]
    public void StartExitVehicle()
    {
        Invoke("ExitVehicle", .25f);
    }

    void ExitVehicle()
    {
        if (_character.state != NPC_States.NPC_STATES.SITTING)
        {
            Debug.LogError("method 'ExitVehicle()' NPC_States.NPC_STATES.SITTING");
            return;
        }

        Sequence exitSeatSequence = DOTween.Sequence();
        _character.vehicleSeatingSlot.slotStatus = SeatObjectSlot.SeatSlotStatus.EMPTY;
        _character.ExitSeat(out exitSeatSequence);

        exitSeatSequence.OnComplete
        (
            () =>
            {
                //Invoke("MoveToVehicleExit", 1f);
                MoveToVehicleExit();

            }
        );
    }

    /// <summary>
    /// Creates Path, moves player to Exit Vehicle
    /// </summary>
    void MoveToVehicleExit()
    {
        _character.state = NPC_States.NPC_STATES.EXITING_VEH;
        _passenger.passengerStatus = PassengerStatusEnum.PassengerStatus.EXITING_VEHICLE;

        _character.transform.SetParent(null);

        Vector3 startPos = transform.position;
        Vector3 islePos = villageExpress.isleTargetTransform.transform.position;
        Vector3 exitPositiom = villageExpress.entranceTargetTransform.transform.position;

        Vector3 exitLookAt = new Vector3(exitPositiom.x, islePos.y, exitPositiom.z);

        Sequence sequence = DOTween.Sequence();

        sequence
        .Append(transform.DOMove(islePos, 1).OnComplete(() => { nPC_MovementController.AnimatorSetSpeed(0); }))
        .Insert(0, transform.DOLookAt(islePos, .5f))
        .Insert(1, transform.DOLookAt(exitLookAt, .5f))
        .Insert(2, transform.DOMove(exitPositiom, 1f).OnStart((() => { nPC_MovementController.AnimatorSetSpeed(.1f); })));

        sequence.OnStart
        (
            () =>
            {
                nPC_MovementController.AnimatorSetSpeed(.1f);
            }
        );

        sequence.OnComplete(OnMoveToVehicleExitCompleted);
    }

    void OnMoveToVehicleExitCompleted()
    {

        nPC_MovementController.AnimatorSetSpeed(0);
        nPC_MovementController.GetNavAgent().enabled = true;

        Vector3 pos = transform.position + transform.forward * 3;
        pos.y = villageExpress.entranceTargetTransform.transform.position.y;

        transform.DOMove(pos, 1)
        .SetDelay(1)
        .OnStart
        (
            () =>
            {
                nPC_MovementController.AnimatorSetSpeed(0.1f);
            }
        )
        .OnComplete
        (
            () =>
            {
                nPC_MovementController.AnimatorSetSpeed(0);
                _character.state = NPC_States.NPC_STATES.IDLE;
                _passenger.passengerStatus = PassengerStatusEnum.PassengerStatus.EXITED_VEHICLE;
                nPC_MovementController.MoveToTargetLocation(_passenger.GetBusStopDestination());
            }
        );

    }


    #endregion
}
