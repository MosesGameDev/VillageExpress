using UnityEngine;
using System.Collections.Generic;

public class SeatMoodStatusUIE : MonoBehaviour
{
    [SerializeField] private List<MoodStatusUIElement> seatMoodStatusUIElements = new List<MoodStatusUIElement>();

    private void OnEnable()
    {
        PlayerSeatInteraction.OnPlayerLookAtSeat += PlayerSeatInteraction_OnPlayerLookAtSeat;
        PlayerInteractionsHandler.OnInteractibleClearHighlight += PlayerInteractionsHandler_OnInteractibleClearHighlight;
    }

    private void PlayerInteractionsHandler_OnInteractibleClearHighlight(Interactible interactible)
    {
        if (seatMoodStatusUIElements[0] == null || seatMoodStatusUIElements[1] == null) return;

        if (!seatMoodStatusUIElements[0].gameObject.activeInHierarchy && !seatMoodStatusUIElements[1].gameObject.activeInHierarchy) return;

        foreach (var seatMoodStatusUIElement in seatMoodStatusUIElements)
        {
            if (seatMoodStatusUIElement != null)
            {
                seatMoodStatusUIElement.gameObject.SetActive(false);
            }
        }
    }

    private void OnDisable()
    {
        PlayerSeatInteraction.OnPlayerLookAtSeat -= PlayerSeatInteraction_OnPlayerLookAtSeat;
        PlayerInteractionsHandler.OnInteractibleHighlighted += PlayerInteractionsHandler_OnInteractibleClearHighlight;
    }

    private void PlayerSeatInteraction_OnPlayerLookAtSeat(List<MoodHandler> moodHandlers)
    {
        for (int i = 0; i < seatMoodStatusUIElements.Count; i++)
        {
            if (i < moodHandlers.Count)
            {
                seatMoodStatusUIElements[i].Initialize(moodHandlers[i].GetComponent<NPCharacter>());
                seatMoodStatusUIElements[i].gameObject.SetActive(true);
            }
            else
            {
                seatMoodStatusUIElements[i].gameObject.SetActive(false);
            }
        }
    }
}
