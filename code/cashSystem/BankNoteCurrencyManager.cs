using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BankNoteCurrencyManager : MonoBehaviour
{
    [SerializeField] private int currentBalance;
    public DragToPickUpBankNote[] bankNotes;

    [Space]
    public PassengerCommute passengerCommute;

    [Space]
    [SerializeField] private TextMeshProUGUI fareCostAmountText;
    [SerializeField] private TextMeshProUGUI paidAmountText;
    [SerializeField] private TextMeshProUGUI changeAmountText;
    [SerializeField] private UIDialogueElement textEffectGood;
    [SerializeField] private UIDialogueElement textEffectWrong;

    public int change;

    public static event Action<int> OnCurrentBalanceUpdated;
    public static event Action<int, int> OnAddCurrency;

    #region Events
    private void OnEnable()
    {
        DragToPickUpBankNote.OnPickUpNote += BankNotePickup_OnPickUpNote;
        DragToPickUpBankNote.OnDropNote += BankNotePickup_OnDropNote;
        Passenger.onPayBusFare += Passenger_onPayBusFare;
    }


    private void Passenger_onPayBusFare(PassengerCommute commute)
    {
        passengerCommute = commute;
        int amountPaid = passengerCommute.GetProvidedAmount();
        change = (amountPaid - passengerCommute.fareCost);


        if (change < 1)
        {
            passengerCommute.passenger.GetCharacter().ToggleNPCInteraction(true);
            return;
        }

        GetComponent<UIDialogueElement>().Show();
        fareCostAmountText.SetText($"Cost: {passengerCommute.fareCost}/-");
        paidAmountText.SetText($"Paid: {amountPaid}/-");
        changeAmountText.SetText($"Change: {change}/-");

        UpdateCurrentBalance();
        AddCurrency(amountPaid, 1);
    }


    private void OnDisable()
    {
        DragToPickUpBankNote.OnPickUpNote -= BankNotePickup_OnPickUpNote;
        DragToPickUpBankNote.OnDropNote -= BankNotePickup_OnDropNote;
        Passenger.onPayBusFare -= Passenger_onPayBusFare;
    }


    private void BankNotePickup_OnPickUpNote(int value)
    {
        currentBalance -= value;
        OnCurrentBalanceUpdated?.Invoke(currentBalance);
    }


    /// <param name="value"></param> Value of the currency note, example 100 note ==  value 100
    /// <param name="count"></param> count is the number of notes of <param name="value"> 
    /// <param name="dropSuccess"></param>
    private void BankNotePickup_OnDropNote(int value, int count, bool dropSuccess)
    {
        if (!dropSuccess)
        {
            AddCurrency(value, 1);
            return;
        }

        change -= value;

        if (change <= 0)
        {
            OnGivePassengerChange();
        }

        changeAmountText.SetText($"Change: {change}/-");
    }

    #endregion


    private void Start()
    {
        UpdateCurrentBalance();
    }


    [Button]
    public void AddCurrency(int bankNoteValue, int count)
    {
        DragToPickUpBankNote notePickup = null;

        for (int i = 0; i < bankNotes.Length; i++)
        {
            if (bankNotes[i].noteValue == bankNoteValue)
            {
                notePickup = bankNotes[i];
                break;
            }
        }

        notePickup.numberOfNotes += count;
        OnAddCurrency?.Invoke(bankNoteValue, notePickup.numberOfNotes);
        UpdateCurrentBalance();
    }

    public void RefundChange(int changeAmount)
    {
        int[] denominations = { 200, 100, 50, 20, 10 };

        List<int> refundNotes = new List<int>();

        foreach (int denomination in denominations)
        {
            int count = changeAmount / denomination;
            if (count > 0)
            {
                refundNotes.Add(denomination);
                refundNotes.Add(count);
                changeAmount -= denomination * count;
            }
        }

        for (int i = 0; i < refundNotes.Count; i += 2)
        {
            int bankNoteValue = refundNotes[i];
            int count = refundNotes[i + 1];
            AddCurrency(bankNoteValue, count);

            DragToPickUpBankNote note = GetDragNote(bankNoteValue);
            note.transform.DOPunchScale(Vector3.one * .1f, .3f).OnComplete(() => { note.transform.localScale = Vector3.one; });
        }
    }

    DragToPickUpBankNote GetDragNote(int value)
    {
        for (int i = 0; i < bankNotes.Length; i++)
        {
            if (bankNotes[i].noteValue == value)
            {
                return bankNotes[i];
            }
        }

        return null;
    }

    void OnGivePassengerChange()
    {
        passengerCommute.passenger.OnRecieveChange();


        if (change < 0)
        {
            int refund = (change *= -1);
            RefundChange(refund);
            change = 0;
            textEffectWrong.Show();
            Invoke("Close", .6f);
            return;
        }

        textEffectGood.Show();
        Invoke("Close", .6f);

    }


    void Close()
    {
        GetComponent<UIDialogueElement>().Hide();
    }

    public void UpdateCurrentBalance()
    {
        int count = 0;

        for (int i = 0; i < bankNotes.Length; i++)
        {
            count += (bankNotes[i].numberOfNotes * bankNotes[i].noteValue);
        }

        currentBalance = count;
        OnCurrentBalanceUpdated?.Invoke(currentBalance);
    }
}

