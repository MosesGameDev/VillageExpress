using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BusStop : MonoBehaviour
{
    public Location busStopLocation;
    [Space]
    [SerializeField] private bool populateOnStart;
    [SerializeField] private int passengersCount;
    [SerializeField] private float minSpawnDistance = 1f;

    [Space]
    [SerializeField] private BoxCollider spawnArea;

    [Header("Character Spawn Chances")]
    public CharacterSpawnChance[] spawnChance;

    [Space]
    [SerializeField] private PassengerCrowd passengers;

    [Space]
    [SerializeField] private MoveToTargetTransform[] destinationMoveToPositions;


    private List<Vector3> occupiedSpawnPositions = new List<Vector3>();
    bool populated;
    SpawnAreaManager spawnManager;

    private void OnEnable()
    {
        VE_Vehicle.OnVehicleStatusChanged += VE_Vehicle_OnVehicleStatusChanged;
        for (int i = 0; i < destinationMoveToPositions.Length; i++)
        {
            destinationMoveToPositions[i].onCharacterReachLocation += DeactivateCharacterOnReachDestination;
        }
    }

    

    IEnumerator Start()
    {
        yield return new WaitForSeconds(1);
        if (populateOnStart)
        {
            PopulatePassengersAtBusStop();
        }
    }

    

    private void Awake()
    {
        spawnManager = new SpawnAreaManager(spawnArea, minSpawnDistance);
    }

    public int GetPassengerCount()
    {
        return passengersCount;
    }

    public PassengerCrowd GetPassengerCrowd()
    {
        return passengers;
    }

    private void VE_Vehicle_OnVehicleStatusChanged(VE_StatusEnum.vE_Status status)
    {
        if (status == VE_StatusEnum.vE_Status.STOPPED)
        {
            if (SceneRegistry.Instance.vE_Vehicle.currentBusStop == this)
            {
                if (busStopLocation != null)
                {

                    print("<color=green>'<b>BUS STOP: </b></color>" + busStopLocation.name + " VE_STOPPED");
                }

                OnBusReachedLocation();
            }
        }
    }

    public MoveToTargetTransform GetRandomBusStopDestination()
    {
        return destinationMoveToPositions[Random.Range(0, destinationMoveToPositions.Length)];
    }

    void OnBusReachedLocation()
    {
        if (passengersCount < 1)
        {
            //No passengers to pickup, vehicle check drops passengers
            return;
        }

        if (passengers.passengerList.Count < 1)
        {
            PopulatePassengersAtBusStop();
        }

    }

    /// <summary>
    /// Gets chareacter grom spawned NPCharacter pool and add them to passenger crowd
    /// </summary>
    [Button]
    public void PopulatePassengersAtBusStop()
    {
        if (populated)
        {
            return;
        }

        occupiedSpawnPositions.Clear();


        int mainCharsToSpawn = 0;
        List<CharacterSpawnChance> mainCharSpawnChances = new List<CharacterSpawnChance>(spawnChance);
        for (int i = 0; i < mainCharSpawnChances.Count; i++)
        {
            if (Random.value <= mainCharSpawnChances[i].spawnChance)
            {
                Passenger mainCharacter = SceneRegistry.Instance.passangerSpawner.GetMainCharacter(mainCharSpawnChances[i].name);
                if (mainCharacter != null)
                {
                    mainCharacter.transform.eulerAngles = transform.eulerAngles;
                    mainCharacter.transform.position = spawnManager.GetSpawnPosition();

                    Location destination = SceneRegistry.Instance.busRouteManager.GetRandomLocation(SceneRegistry.Instance.busRouteManager.GetLocationIndex(busStopLocation));
                    mainCharacter.commute = new PassengerCommute(this, destination.busStop, SceneRegistry.Instance.busRouteManager.CalculateCommuteCost(busStopLocation, destination));
                    string id = $"{mainCharacter.GetCharacter().characterName}_{busStopLocation.locationName}>{destination.locationName}";
                    mainCharacter.gameObject.name = id;

                    passengers.passengerList.Add(mainCharacter);
                    mainCharsToSpawn++;
                }
            }
        }

        // Spawn pooled characters
        for (int i = 0; i < passengersCount - mainCharsToSpawn; i++)
        {
            Passenger passenger = SceneRegistry.Instance.passangerSpawner.ObjectPool.Get();
            passenger.transform.eulerAngles = transform.eulerAngles;
            passenger.transform.position = spawnManager.GetSpawnPosition();

            Location destination = SceneRegistry.Instance.busRouteManager.GetRandomLocation(SceneRegistry.Instance.busRouteManager.GetLocationIndex(busStopLocation));
            passenger.commute = new PassengerCommute(this, destination.busStop, SceneRegistry.Instance.busRouteManager.CalculateCommuteCost(busStopLocation, destination));
            string id = $"{i}_{passenger.GetCharacter().characterName}_{busStopLocation.locationName}>{destination.locationName}";
            passenger.gameObject.name = id;

            passengers.passengerList.Add(passenger);
        }

        populated = true;
    }

    void DeactivateCharacterOnReachDestination(NPCharacter character)
    {
        SceneRegistry.Instance.passangerSpawner.ObjectPool.Release(character.GetComponent<Passenger>());
    }

    /// <summary>
    /// Get random position within collider to spawn characters
    /// </summary>
    /// <param name="boxCollider"></param>
    /// <returns></returns>
    /// <summary>
    /// Get random position within collider to spawn characters
    /// Ensures spawn positions do not overlap
    /// </summary>
    public Vector3 GetRandomPointInsideCollider(BoxCollider boxCollider)
    {
        int maxAttempts = 50;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector3 _extents = boxCollider.size / 2f;
            Vector3 point = new Vector3(
                Random.Range(-_extents.x, _extents.x),
                Random.Range(-_extents.y, _extents.y),
                Random.Range(-_extents.z, _extents.z)
            ) + boxCollider.center; // Add the collider's center offset

            Vector3 worldPoint = boxCollider.transform.TransformPoint(point);

            bool isValidPosition = true;
            foreach (Vector3 existingPosition in occupiedSpawnPositions)
            {
                if (Vector3.Distance(worldPoint, existingPosition) < minSpawnDistance)
                {
                    isValidPosition = false;
                    break;
                }
            }

            if (isValidPosition)
            {
                occupiedSpawnPositions.Add(worldPoint);
                return worldPoint;
            }
        }

        // Fallback position also needs to account for center offset
        Vector3 extents = boxCollider.size / 2f;
        Vector3 fallbackPoint = boxCollider.transform.TransformPoint(new Vector3(
            Random.Range(-extents.x, extents.x),
            Random.Range(-extents.y, extents.y),
            Random.Range(-extents.z, extents.z)
        ) + boxCollider.center);

        occupiedSpawnPositions.Add(fallbackPoint);
        return fallbackPoint;
    }
    private void OnDisable()
    {
        occupiedSpawnPositions.Clear();
        VE_Vehicle.OnVehicleStatusChanged -= VE_Vehicle_OnVehicleStatusChanged;
        for (int i = 0; i < destinationMoveToPositions.Length; i++)
        {
            destinationMoveToPositions[i].onCharacterReachLocation -= DeactivateCharacterOnReachDestination;
        }
    }
}

[System.Serializable]
public class CharacterSpawnChance
{
    public string name;
    [Range(0, 1)] public float spawnChance;
}

public class SpawnAreaManager
{
    private readonly HashSet<Vector3> occupiedPositions = new HashSet<Vector3>();
    private readonly float minDistance;
    private readonly int maxAttempts;
    private readonly BoxCollider spawnArea;

    public SpawnAreaManager(BoxCollider area, float minSpawnDistance, int maxSpawnAttempts = 50)
    {
        spawnArea = area;
        minDistance = minSpawnDistance;
        maxAttempts = maxSpawnAttempts;
    }

    public void Clear() => occupiedPositions.Clear();

    public Vector3 GetSpawnPosition()
    {
        Vector3 bounds = spawnArea.size;
        Vector3 center = spawnArea.center;

        // Use a grid-based approach for better distribution
        float cellSize = minDistance * 1.5f;
        int xCells = Mathf.FloorToInt(bounds.x / cellSize);
        int zCells = Mathf.FloorToInt(bounds.z / cellSize);

        // Try to find a valid position
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Get a random cell
            int xCell = Random.Range(0, xCells);
            int zCell = Random.Range(0, zCells);

            // Convert to position with some random offset within cell
            Vector3 basePos = new Vector3(
                (xCell * cellSize) - (bounds.x * 0.5f) + Random.Range(0, cellSize),
                0,
                (zCell * cellSize) - (bounds.z * 0.5f) + Random.Range(0, cellSize)
            ) + center;

            Vector3 worldPos = spawnArea.transform.TransformPoint(basePos);

            if (IsValidPosition(worldPos))
            {
                occupiedPositions.Add(worldPos);
                return worldPos;
            }
        }

        // Fallback to a random position if no valid position found
        return GetFallbackPosition();
    }

    private bool IsValidPosition(Vector3 position)
    {
        foreach (Vector3 occupied in occupiedPositions)
        {
            if (Vector3.Distance(position, occupied) < minDistance)
            {
                return false;
            }
        }
        return true;
    }

    private Vector3 GetFallbackPosition()
    {
        Vector3 extents = spawnArea.size / 2f;
        Vector3 randomPoint = new Vector3(
            Random.Range(-extents.x, extents.x),
            0,
            Random.Range(-extents.z, extents.z)
        ) + spawnArea.center;

        Vector3 worldPoint = spawnArea.transform.TransformPoint(randomPoint);
        occupiedPositions.Add(worldPoint);
        return worldPoint;
    }
}