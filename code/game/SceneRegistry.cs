using UnityEngine;

public class SceneRegistry : MonoBehaviour
{
    public static SceneRegistry Instance;

    public VE_Vehicle vE_Vehicle;
    public PassengerCharacterSpawner passangerSpawner;
    public NPC_HUD_UIElementPoolManager npcHudSpawner;
    public BusRouteManager busRouteManager;
    public PlayerRefrences player;

    private void Awake()
    {
        Instance = this;
    }
}
