using Sirenix.OdinInspector;
using System;
using UnityEngine;

public class MoodHandler : MonoBehaviour
{
    [Header("Mood Properties")]
    [Range(-100f, 100f)] public float moodValue = 0f;
    public MoodState currentMood;

    [Header("Mood Behavior")]
    public float moodChangeRate = 1f;

    [Header("Influence Factors")]
    public MusicPreference musicPreference = MusicPreference.None;
    [Space]
    [ReadOnly] public MusicPreference currentMusic = MusicPreference.Off;
    [ReadOnly] public float hygieneLevel = 100f;
    [ReadOnly] public bool isInConflict = false;
    [ReadOnly] public float globalConflictLevel = 0f;

    public event Action<MoodState> OnMoodStateChanged;
    public event Action<float> OnMoodValueChanged;

    void OnDisable()
    {
        MoodSystemManager.Instance?.UnregisterPassenger(this);
    }

    private void Start()
    {
        MoodSystemManager.Instance?.RegisterPassenger(this);
        InitializeMood();
    }

    void InitializeMood()
    {
        if (MoodSystemManager.Instance == null) return;

        currentMood = MoodSystemManager.Instance.GetRandomStartingMood();
        moodValue = MoodSystemManager.Instance.GetMoodValueFromState(currentMood);

        OnMoodValueChanged?.Invoke(moodValue);
        OnMoodStateChanged?.Invoke(currentMood);
    }

    public void UpdateMoodTick()
    {
        float delta = 0f;

        moodValue = Mathf.Clamp(moodValue + (delta * moodChangeRate), -100f, 100f);

        var newMood = MoodSystemManager.Instance.DetermineMoodState(moodValue);
        if (newMood != currentMood)
        {
            currentMood = newMood;
            OnMoodStateChanged?.Invoke(newMood);
        }

        OnMoodValueChanged?.Invoke(moodValue);
    }




    float GetMusicInfluence()
    {
        if (currentMusic == MusicPreference.Off || musicPreference == MusicPreference.None)
            return 0f;

        if (currentMusic == musicPreference)
            return 1.5f;
        else
            return -0.5f;
    }

    float GetHygieneInfluence()
    {
        if (hygieneLevel < 30f)
            return -2f;
        else if (hygieneLevel < 60f)
            return -1f;
        else if (hygieneLevel > 80f)
            return +0.5f;

        return 0f;
    }

    float GetConflictInfluence()
    {
        if (isInConflict) return -3f;
        return -globalConflictLevel * 0.5f;
    }

}
