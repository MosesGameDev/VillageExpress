using Sirenix.OdinInspector;
using UnityEngine;

public class NPC_HUDUIElement : MonoBehaviour
{
    public CanvasGroup canvasGroup; // The canvas group for the HUD element

    [ReadOnly] public NPC_HUD npcHUD;
    public PassengerBoardingHUD passengerBoardingHUD;

    public void Initialize()
    {
        passengerBoardingHUD.Hide();
    }
}

