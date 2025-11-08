using UnityEngine;
using UnityEngine.Pool;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class PassengerCharacterSpawner : MonoBehaviour
{
    [System.Serializable]
    public class MainGameCharacterPassenger
    {
        public string id;
        public Passenger passengerPrefab;
    }

    [Title("Main Characters")]
    [SerializeField] private MainGameCharacterPassenger[] mainGameCharacters;

    [Title("Pooled Characters")]
    [SerializeField] private Passenger[] passengerPrefabs;

    private Dictionary<string, Passenger> spawnedMainCharacters = new Dictionary<string, Passenger>();
    public IObjectPool<Passenger> ObjectPool { get; private set; }

    [Title("Pool Settings")]
    [SerializeField] private int defaultPoolSize = 15;
    [SerializeField] private int maxPoolSize = 25;
    [SerializeField] private bool collectionsCheck = false;

    private void Awake()
    {
        ObjectPool = new ObjectPool<Passenger>(
            CreatePooledItem,
            OnTakeFromPool,
            OnReturnedToPool,
            OnDestroyPoolObject,
            collectionsCheck,
            defaultPoolSize,
            maxPoolSize
        );

        InitializeMainCharacters();
    }

    private void InitializeMainCharacters()
    {
        foreach (var mainChar in mainGameCharacters)
        {
            if (!string.IsNullOrEmpty(mainChar.id) && mainChar.passengerPrefab != null)
            {
                if (!spawnedMainCharacters.ContainsKey(mainChar.id))
                {
                    var instance = Instantiate(mainChar.passengerPrefab);
                    instance.gameObject.SetActive(false);
                    spawnedMainCharacters.Add(mainChar.id, instance);
                }
            }
        }
    }

    public Passenger GetMainCharacter(string characterId)
    {
        if (spawnedMainCharacters.TryGetValue(characterId, out Passenger mainCharacter))
        {
            NPCharacter character = mainCharacter.GetComponent<Passenger>().GetCharacter();
            mainCharacter.gameObject.SetActive(true);
            character.EnableCharacter();
            return mainCharacter;
        }

        Debug.LogWarning($"Main character with ID {characterId} not found!");
        return null;
    }

    public void ReturnMainCharacter(string characterId)
    {
        if (spawnedMainCharacters.TryGetValue(characterId, out Passenger mainCharacter))
        {
            NPCharacter character = mainCharacter.GetComponent<Passenger>().GetCharacter();
            character.DisableCharacter();
            mainCharacter.gameObject.SetActive(false);
        }
    }

    private Passenger CreatePooledItem()
    {
        // Randomly select from available passenger prefabs
        Passenger prefab = passengerPrefabs[Random.Range(0, passengerPrefabs.Length)];
        Passenger passenger = Instantiate(prefab);
        return passenger;
    }

    private void OnReturnedToPool(Passenger passenger)
    {
        NPCharacter character = passenger.GetComponent<Passenger>().GetCharacter();
        character.DisableCharacter();
        passenger.gameObject.SetActive(false);
    }

    private void OnTakeFromPool(Passenger passenger)
    {
        NPCharacter character = passenger.GetComponent<Passenger>().GetCharacter();
        passenger.commute.passenger = passenger;
        character.EnableCharacter();
        passenger.gameObject.SetActive(true);
    }

    private void OnDestroyPoolObject(Passenger passenger)
    {
        Destroy(passenger.gameObject);
    }

    private void OnDestroy()
    {
        // Clean up spawned main characters
        foreach (var mainChar in spawnedMainCharacters.Values)
        {
            if (mainChar != null)
            {
                Destroy(mainChar.gameObject);
            }
        }
        spawnedMainCharacters.Clear();
    }
}