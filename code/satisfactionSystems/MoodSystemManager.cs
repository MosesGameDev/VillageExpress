using System.Collections.Generic;
using UnityEngine;

public class MoodSystemManager : MonoBehaviour
{
    [Tooltip("How often NPC moods update (in seconds)")]
    [SerializeField] private float tickInterval = 1f;

    [Space]
    public float influenceStrength = 0.5f;

    private float timer = 0f;
    [SerializeField] private List<MoodHandler> passengers = new List<MoodHandler>();

    [Header("Global Mood Influences")]
    [Range(0, 100)] public float busHygiene = 100f;
    [Range(0, 100)] public float busConflictLevel = 0f;
    public MusicPreference currentBusMusic = MusicPreference.Off;

    [Space]
    [SerializeField] private SeatObject[] seats;


    public static MoodSystemManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void RegisterPassenger(MoodHandler passenger)
    {
        if (!passengers.Contains(passenger))
            passengers.Add(passenger);
    }

    public void UnregisterPassenger(MoodHandler passenger)
    {
        if (passengers.Contains(passenger))
            passengers.Remove(passenger);
    }

    public MoodState GetRandomStartingMood()
    {
        int randomIndex = Random.Range(0, System.Enum.GetValues(typeof(MoodState)).Length);
        return (MoodState)randomIndex;
    }


    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= tickInterval)
        {
            timer = 0f;
            UpdateAllPassengers();
        }
    }

    private void UpdateAllPassengers()
    {
        print("Updating Passenger Moods");
        foreach (SeatObject seat in seats)
        {
            seat.UpdatePassengerMood();
        }

        foreach (var p in passengers)
        {
            if (p == null) continue;

            p.currentMusic = currentBusMusic;
            p.hygieneLevel = busHygiene;
            p.globalConflictLevel = busConflictLevel;
            p.UpdateMoodTick();
        }
    }


    public MoodState DetermineMoodState(float value)
    {
        if (value <= -80) return MoodState.Mad;
        if (value <= -60) return MoodState.Angry;
        if (value <= -40) return MoodState.Frustrated;
        if (value <= -20) return MoodState.Upset;
        if (value < 0) return MoodState.Uneasy;
        if (value == 0) return MoodState.Calm;
        if (value <= 20) return MoodState.Vibing;
        if (value <= 40) return MoodState.Happy;
        if (value <= 60) return MoodState.Excited;
        if (value <= 80) return MoodState.Hyped;
        return MoodState.Ecstatic;
    }

    public float GetMoodValueFromState(MoodState state)
    {
        return state switch
        {
            MoodState.Mad => Random.Range(-100, -80),
            MoodState.Angry => Random.Range(-80, -60),
            MoodState.Frustrated => Random.Range(-60, -40),
            MoodState.Upset => Random.Range(-40, -20),
            MoodState.Uneasy => Random.Range(-20, 0),
            MoodState.Calm => 0,
            MoodState.Vibing => Random.Range(0, 20),
            MoodState.Happy => Random.Range(20, 40),
            MoodState.Excited => Random.Range(40, 60),
            MoodState.Hyped => Random.Range(60, 80),
            MoodState.Ecstatic => Random.Range(80, 100),
            _ => 0
        };
    }


    public float GetMatrixInfluence(MoodState a, MoodState b)
    {
        if (moodMatrix.TryGetValue((a, b), out float delta))
            return delta;
        return 0f;
    }

    private static readonly Dictionary<(MoodState, MoodState), float> moodMatrix = new()
    {
        // M1 Mad row
        {(MoodState.Mad, MoodState.Mad), -5f},
        {(MoodState.Mad, MoodState.Angry), -4f},
        {(MoodState.Mad, MoodState.Frustrated), -3f},
        {(MoodState.Mad, MoodState.Upset), -2f},
        {(MoodState.Mad, MoodState.Uneasy), -1f},
        {(MoodState.Mad, MoodState.Calm), +1f},
        {(MoodState.Mad, MoodState.Vibing), +2f},
        {(MoodState.Mad, MoodState.Happy), +2f},
        {(MoodState.Mad, MoodState.Excited), +3f},
        {(MoodState.Mad, MoodState.Hyped), +3f},
        {(MoodState.Mad, MoodState.Ecstatic), +3f},

        // M2 Angry row
        {(MoodState.Angry, MoodState.Mad), -4f},
        {(MoodState.Angry, MoodState.Angry), -4f},
        {(MoodState.Angry, MoodState.Frustrated), -3f},
        {(MoodState.Angry, MoodState.Upset), -2f},
        {(MoodState.Angry, MoodState.Uneasy), -1f},
        {(MoodState.Angry, MoodState.Calm), +1f},
        {(MoodState.Angry, MoodState.Vibing), +2f},
        {(MoodState.Angry, MoodState.Happy), +2f},
        {(MoodState.Angry, MoodState.Excited), +3f},
        {(MoodState.Angry, MoodState.Hyped), +3f},
        {(MoodState.Angry, MoodState.Ecstatic), +4f},

        // M3 Frustrated row
        {(MoodState.Frustrated, MoodState.Mad), -3f},
        {(MoodState.Frustrated, MoodState.Angry), -3f},
        {(MoodState.Frustrated, MoodState.Frustrated), -2f},
        {(MoodState.Frustrated, MoodState.Upset), -1f},
        {(MoodState.Frustrated, MoodState.Uneasy), 0f},
        {(MoodState.Frustrated, MoodState.Calm), +1f},
        {(MoodState.Frustrated, MoodState.Vibing), +2f},
        {(MoodState.Frustrated, MoodState.Happy), +2f},
        {(MoodState.Frustrated, MoodState.Excited), +3f},
        {(MoodState.Frustrated, MoodState.Hyped), +4f},
        {(MoodState.Frustrated, MoodState.Ecstatic), +4f},

        // M4 Upset row
        {(MoodState.Upset, MoodState.Mad), -2f},
        {(MoodState.Upset, MoodState.Angry), -2f},
        {(MoodState.Upset, MoodState.Frustrated), -1f},
        {(MoodState.Upset, MoodState.Upset), -1f},
        {(MoodState.Upset, MoodState.Uneasy), 0f},
        {(MoodState.Upset, MoodState.Calm), +1f},
        {(MoodState.Upset, MoodState.Vibing), +2f},
        {(MoodState.Upset, MoodState.Happy), +3f},
        {(MoodState.Upset, MoodState.Excited), +3f},
        {(MoodState.Upset, MoodState.Hyped), +4f},
        {(MoodState.Upset, MoodState.Ecstatic), +4f},

        // M5 Uneasy row
        {(MoodState.Uneasy, MoodState.Mad), -1f},
        {(MoodState.Uneasy, MoodState.Angry), -1f},
        {(MoodState.Uneasy, MoodState.Frustrated), 0f},
        {(MoodState.Uneasy, MoodState.Upset), 0f},
        {(MoodState.Uneasy, MoodState.Uneasy), 0f},
        {(MoodState.Uneasy, MoodState.Calm), +1f},
        {(MoodState.Uneasy, MoodState.Vibing), +2f},
        {(MoodState.Uneasy, MoodState.Happy), +3f},
        {(MoodState.Uneasy, MoodState.Excited), +3f},
        {(MoodState.Uneasy, MoodState.Hyped), +4f},
        {(MoodState.Uneasy, MoodState.Ecstatic), +5f},

        // M6 Calm row
        {(MoodState.Calm, MoodState.Mad), -1f},
        {(MoodState.Calm, MoodState.Angry), -1f},
        {(MoodState.Calm, MoodState.Frustrated), 0f},
        {(MoodState.Calm, MoodState.Upset), 0f},
        {(MoodState.Calm, MoodState.Uneasy), +1f},
        {(MoodState.Calm, MoodState.Calm), 0f},
        {(MoodState.Calm, MoodState.Vibing), +1f},
        {(MoodState.Calm, MoodState.Happy), +2f},
        {(MoodState.Calm, MoodState.Excited), +3f},
        {(MoodState.Calm, MoodState.Hyped), +3f},
        {(MoodState.Calm, MoodState.Ecstatic), +4f},

        // M7 Vibing row
        {(MoodState.Vibing, MoodState.Mad), -2f},
        {(MoodState.Vibing, MoodState.Angry), -2f},
        {(MoodState.Vibing, MoodState.Frustrated), -1f},
        {(MoodState.Vibing, MoodState.Upset), 0f},
        {(MoodState.Vibing, MoodState.Uneasy), +1f},
        {(MoodState.Vibing, MoodState.Calm), +1f},
        {(MoodState.Vibing, MoodState.Vibing), +1f},
        {(MoodState.Vibing, MoodState.Happy), +2f},
        {(MoodState.Vibing, MoodState.Excited), +3f},
        {(MoodState.Vibing, MoodState.Hyped), +4f},
        {(MoodState.Vibing, MoodState.Ecstatic), +4f},

        // M8 Happy row
        {(MoodState.Happy, MoodState.Mad), -3f},
        {(MoodState.Happy, MoodState.Angry), -2f},
        {(MoodState.Happy, MoodState.Frustrated), -1f},
        {(MoodState.Happy, MoodState.Upset), 0f},
        {(MoodState.Happy, MoodState.Uneasy), +1f},
        {(MoodState.Happy, MoodState.Calm), +2f},
        {(MoodState.Happy, MoodState.Vibing), +2f},
        {(MoodState.Happy, MoodState.Happy), +2f},
        {(MoodState.Happy, MoodState.Excited), +3f},
        {(MoodState.Happy, MoodState.Hyped), +4f},
        {(MoodState.Happy, MoodState.Ecstatic), +5f},

        // M9 Excited row
        {(MoodState.Excited, MoodState.Mad), -3f},
        {(MoodState.Excited, MoodState.Angry), -3f},
        {(MoodState.Excited, MoodState.Frustrated), -2f},
        {(MoodState.Excited, MoodState.Upset), 0f},
        {(MoodState.Excited, MoodState.Uneasy), +1f},
        {(MoodState.Excited, MoodState.Calm), +2f},
        {(MoodState.Excited, MoodState.Vibing), +3f},
        {(MoodState.Excited, MoodState.Happy), +3f},
        {(MoodState.Excited, MoodState.Excited), +3f},
        {(MoodState.Excited, MoodState.Hyped), +4f},
        {(MoodState.Excited, MoodState.Ecstatic), +5f},

        // M10 Hyped row
        {(MoodState.Hyped, MoodState.Mad), -3f},
        {(MoodState.Hyped, MoodState.Angry), -3f},
        {(MoodState.Hyped, MoodState.Frustrated), -2f},
        {(MoodState.Hyped, MoodState.Upset), 0f},
        {(MoodState.Hyped, MoodState.Uneasy), +2f},
        {(MoodState.Hyped, MoodState.Calm), +3f},
        {(MoodState.Hyped, MoodState.Vibing), +3f},
        {(MoodState.Hyped, MoodState.Happy), +4f},
        {(MoodState.Hyped, MoodState.Excited), +4f},
        {(MoodState.Hyped, MoodState.Hyped), +4f},
        {(MoodState.Hyped, MoodState.Ecstatic), +5f},

        // M11 Ecstatic row
        {(MoodState.Ecstatic, MoodState.Mad), -3f},
        {(MoodState.Ecstatic, MoodState.Angry), -3f},
        {(MoodState.Ecstatic, MoodState.Frustrated), -2f},
        {(MoodState.Ecstatic, MoodState.Upset), 0f},
        {(MoodState.Ecstatic, MoodState.Uneasy), +2f},
        {(MoodState.Ecstatic, MoodState.Calm), +3f},
        {(MoodState.Ecstatic, MoodState.Vibing), +3f},
        {(MoodState.Ecstatic, MoodState.Happy), +4f},
        {(MoodState.Ecstatic, MoodState.Excited), +4f},
        {(MoodState.Ecstatic, MoodState.Hyped), +5f},
        {(MoodState.Ecstatic, MoodState.Ecstatic), +5f},
    };


}


public enum MoodState
{
    Mad,
    Angry,
    Frustrated,
    Upset,
    Uneasy,
    Calm,
    Vibing,
    Happy,
    Excited,
    Hyped,
    Ecstatic
}

public enum MusicPreference
{
    None,
    Off,
    HipHop,
    Reggae,
    Classic,
    Dance,
    AfroBeat
}