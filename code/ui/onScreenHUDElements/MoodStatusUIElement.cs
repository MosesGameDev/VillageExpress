using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class MoodStatusUIElement : MonoBehaviour
{
    [System.Serializable]
    public struct MoodIcon
    {
        public MoodState mood;
        public Sprite icon;
    }

    [SerializeField] bool InitializeOnStart = false;
    [SerializeField] bool isInitialized = false;

    [Space]
    public Image portraitImage;
    public Image moodIconImage;

    [Space]
    public TextMeshProUGUI busFareStatusText;
    public TextMeshProUGUI musicPrefrencesText;
    public TextMeshProUGUI moodStatusText;

    [Space]
    public Gradient positiveMoodStatusGradient;
    public Gradient negativeMoodStatusGradient;

    [Space]
    public MoodIcon[] moodIcons;

    MoodHandler moodHandler;
    float moodValue;
    NPCharacter npc;


    public void Initialize(NPCharacter _npc)
    {
        npc = _npc;
        moodHandler = npc.GetComponent<MoodHandler>();
        if (moodHandler == null)
        {
            Debug.LogError("MoodHandler component not found on parent NPC_HUDUIElement.");
            return;
        }

        //if (isInitialized)
        //    return;

        moodHandler.OnMoodValueChanged += SetMoodValue;

        SetPortraitImage();
        UpdateMoodStatus(moodHandler.currentMood);

        isInitialized = true;
        GetComponent<CanvasGroup>().alpha = 1f; 
    }

    public Sprite GetIcon(MoodState mood)
    {
        foreach (var entry in moodIcons)
            if (entry.mood == mood)
                return entry.icon;
        return null;
    }

    public void SetPortraitImage()
    {
        DialogueActor actor = npc.GetComponent<DialogueActor>();

        if (actor == null)
        {
            Debug.LogWarning("DialogueActor or portrait not found!");
            return;
        }

        portraitImage.sprite = DialogueManager.instance.MasterDatabase.GetActor(actor.GetActorName()).GetPortraitSprite();
    }

    public void UpdateMoodStatus(MoodState newMood)
    {
        moodStatusText.text = newMood.ToString();
        var icon = GetIcon(newMood);
        moodIconImage.sprite = icon;

        if (moodValue < 0)
        {
            moodStatusText.color = negativeMoodStatusGradient.Evaluate(((moodValue * -1) + 100f) / 200f);
        }
        else 
        {
            moodStatusText.color = positiveMoodStatusGradient.Evaluate((moodValue + 100f) / 200f);
        }

        if (npc.GetComponent<Passenger>().commute.fareStatus == PassengerCommute.PassengerCommuteEnum.PENDING)
        {
            busFareStatusText.text = $"<color=red>{npc.GetComponent<Passenger>().commute.fareStatus}  {npc.GetComponent<Passenger>().commute.fareCost.ToString()} </color> KES";
        }
        else if (npc.GetComponent<Passenger>().commute.fareStatus == PassengerCommute.PassengerCommuteEnum.PAID)
        {
            busFareStatusText.text = $"<color=green>{npc.GetComponent<Passenger>().commute.fareStatus} </color>";
        }
    }

    public void SetMoodValue(float newValue)
    {
        moodValue = newValue;
        MoodState mood = MoodSystemManager.Instance.DetermineMoodState(moodValue);
        UpdateMoodStatus(mood);
    }


    private void OnDisable()
    {
        moodHandler.OnMoodValueChanged -= SetMoodValue;
    }
}
