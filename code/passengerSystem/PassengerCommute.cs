using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PassengerCommute
{
    public enum PassengerCommuteEnum { PENDING, PAID }

    public BusStop pickup;
    public BusStop destination;

    [Space]
    public Passenger passenger;
    public int fareCost;
    public int providedAmount;
    public PassengerCommuteEnum fareStatus;

    public int GetProvidedAmount()
    {
        return SetProvidedAmount();
    }

    /// <summary>
    /// The value of the notes the passenger provides to pay for busfare
    /// </summary>
    /// <returns></returns>
    public int SetProvidedAmount()
    {
        int[] ints = new int[] { 10, 20, 50, 100, 200 };
        List<int> filteredInts = new List<int>();

        foreach (int amount in ints)
        {
            if (amount >= fareCost)
            {
                filteredInts.Add(amount);
            }
        }

        return filteredInts[Random.Range(0, filteredInts.Count)];
    }


    public PassengerCommute(BusStop _pickup, BusStop _destination, int _fareCost)
    {
        pickup = _pickup;
        destination = _destination;
        fareCost = _fareCost;
    }

}
