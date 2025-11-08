using Sirenix.OdinInspector;
using UnityEngine;

public class NPC_HUD : MonoBehaviour
{
    public Transform HUDTransform; // The transform that will be used to position the HUD

    [Space]
    public bool assignHudOnStart = true;
    public Renderer meshRenderer; // The renderer of the NPC to check visibility
    public Camera cam; // The camera used to check visibility

    public Vector3 offset; // Offset to apply to the HUD position
    [ReadOnly] public NPC_HUDUIElement npcHUDUIElement; // The UI element that will be used for the HUB


    private void Start()
    {
        if(SceneRegistry.Instance == null)
        {
            return;
        }

        cam  = SceneRegistry.Instance.player.playerCamera;

        if (assignHudOnStart)
        {
            AssignHudElement();
        }

    }


    [Button("Assign HUD Element")]
    public void AssignHudElement()
    {
        if (!npcHUDUIElement)
        {
            SceneRegistry.Instance.npcHudSpawner.ObjectPool.Get(out npcHUDUIElement);
            npcHUDUIElement.npcHUD = this;
            npcHUDUIElement.passengerBoardingHUD.passenger = GetComponent<Passenger>();
            npcHUDUIElement.Initialize();
        }
    }

    public void ReleaseHudElement()
    {
        if (npcHUDUIElement != null)
        {
            SceneRegistry.Instance.npcHudSpawner.ObjectPool.Release(npcHUDUIElement);
            npcHUDUIElement = null;
        }
    }

    [Button("Update HUD Position")]
    public void UpdateHudPosition()
    {
        if(!IsVisible())
        {
            if (npcHUDUIElement != null)
            {
                npcHUDUIElement.canvasGroup.alpha = 0f; // Hide the HUD if not visible
            }
            return;
        }
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, HUDTransform.position + offset);
        npcHUDUIElement.GetComponent<RectTransform>().anchoredPosition = screenPoint;
        npcHUDUIElement.canvasGroup.alpha = 1f; // Show the HUD if visible

    }

    bool IsVisible()
    {
        return GeometryUtility.TestPlanesAABB(
            GeometryUtility.CalculateFrustumPlanes(cam),
            meshRenderer.bounds
        );
    }

}
