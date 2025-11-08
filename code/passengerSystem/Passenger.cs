using System;
using UnityEngine;
using static PassengerStatusEnum;

public class Passenger : MonoBehaviour
{
    public PassengerStatus passengerStatus;

    [Space]
    [SerializeField] private PickUpItemInteractionHandler[] bankNotes;

    PickUpItemInteractionHandler bankNotePickup;
    NPCharacter character;

    [HideInInspector]
    public PassengerCommute commute;

    /// <summary>
    /// fare payment canvas is displayed when this is Invoked;
    /// </summary>
    public static event Action<PassengerCommute> onPayBusFare;
    public static event Action<Passenger> onrecieveChange;


    public MoveToTargetTransform GetBusStopDestination()
    {
        return commute.destination.GetRandomBusStopDestination();
    }

    public void PayBusFare()
    {
        if(character.vehicleSeatingSlot.owningSeat.seatPosition == SeatObject.SeatPosition.RIGHT)
        {
            bankNotePickup = bankNotes[0];
            GetAnimator().CrossFade("Anim_rpm_Male_pay_RIGHT", 0.2f);
        }

        if (character.vehicleSeatingSlot.owningSeat.seatPosition == SeatObject.SeatPosition.LEFT)
        {
            bankNotePickup = bankNotes[1];
            GetAnimator().CrossFade("Anim_rpm_Male_pay_LEFT", 0.2f);
        }


        bankNotePickup.onPickupItem += OnPayBusFare;
        bankNotePickup.gameObject.SetActive(true);

        ///Disable character Interactions, this is for the player to pickup bank note
        character.ToggleNPCInteraction(false);
        character.proceduralAnimController.EnableLookAnimator(SceneRegistry.Instance.player.lookAtTarget, false);
    }

    public NPCharacter GetCharacter()
    {
        if(!character)
        {
            character = GetComponent<NPCharacter>();
        }
        return character;
    }


    public void OnPayBusFare()
    {
        if (!character)
        {
            character = GetComponent<NPCharacter>();
        }

        GetAnimator().CrossFade("Anim_rpm_Male_siting_0", .2f);

        commute.fareStatus = PassengerCommute.PassengerCommuteEnum.PAID;

        /// display fare payment canvas
        /// Mouse sensitivity (FPS controller) is set to 0;
        /// Disable all other interactions on the current seat object
        onPayBusFare?.Invoke(commute);

        /// The colliders are needed for GrabPassengerInteraction
        //character.vehicleSeatingSlot.owningSeat.ToggleEnablePassengerColliders(false);
    }

    public void OnRecieveChange()
    {
        if (!character)
        {
            character = GetComponent<NPCharacter>();
        }

        onrecieveChange?.Invoke(this);
        //character.vehicleSeatingSlot.owningSeat.ToggleEnablePassengerColliders(true);

        //disable look at procedural Anim
        character.proceduralAnimController.DisableLookAnimator(false);

    }

    public Animator GetAnimator()
    {
        return GetComponent<Animator>();
    }

}
