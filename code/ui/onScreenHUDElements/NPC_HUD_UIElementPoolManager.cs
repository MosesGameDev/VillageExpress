using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;


//This script pools PassengerHUDUIElement, PassengerHUDUIElement are created and track the passenger position
// 
public class NPC_HUD_UIElementPoolManager : MonoBehaviour
{
    bool isUpdateElementPositons = false;


    public RectTransform canvas;
    public NPC_HUDUIElement npcHUDPrefab;

    private List<NPC_HUDUIElement> spawnedUIElements = new List<NPC_HUDUIElement>();
    public IObjectPool<NPC_HUDUIElement> ObjectPool { get; private set; }

    [Title("Pool Settings")]
    [SerializeField] private int defaultPoolSize = 15;
    [SerializeField] private int maxPoolSize = 25;
    [SerializeField] private bool collectionsCheck = false;


    private void Awake()
    {
        ObjectPool = new ObjectPool<NPC_HUDUIElement>(
            CreatePooledItem,
            OnTakeFromPool,
            OnReturnedToPool,
            OnDestroyPoolObject,
            collectionsCheck,
            defaultPoolSize,
            maxPoolSize
        );

    }

    private NPC_HUDUIElement CreatePooledItem()
    {
        var instance = Instantiate(npcHUDPrefab, canvas);
        instance.gameObject.SetActive(false);

        return instance;
    }

    private void OnTakeFromPool(NPC_HUDUIElement element)
    {
        element.gameObject.SetActive(true);
        spawnedUIElements.Add(element);
    }

    private void OnReturnedToPool(NPC_HUDUIElement element)
    {
        element.gameObject.SetActive(false);
        if (spawnedUIElements.Contains(element))
        {
            spawnedUIElements.Remove(element);
        }
    }

    private void OnDestroyPoolObject(NPC_HUDUIElement element)
    {
        Destroy(element.gameObject);
    }


    private void LateUpdate()
    {
        if(spawnedUIElements.Count < 1)
        {
            return;
        }

        foreach (var ui in spawnedUIElements)
        {
            ui.npcHUD.UpdateHudPosition();
            isUpdateElementPositons = true;
        }
    }

}
