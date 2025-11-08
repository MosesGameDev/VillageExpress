using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;
using Sirenix.OdinInspector;
using DG.Tweening;

public class NPCEncounterTracker : MonoBehaviour
{
    [System.Serializable]
    public class EncounterEvent
    {
        public int encounters;
        public string playConversationID;
        public bool rotatePlayerCamera = true;
    }

    public string npcName;
    public int playEncounters;
    public bool saveData;

    [Space]
    [SerializeField] private EncounterEvent[] conversations;

    EncounterEvent currentEncounter = null;
    Collider triggerCollider;

    private void Start()
    {
        if (saveData)
        {
            playEncounters = PlayerPrefs.GetInt($"{npcName}-PLAYER-ENCOUNTERS", 0);
        }
        triggerCollider = GetComponent<Collider>();
    }

    [Button]
    public void ClearTrackedEncounters()
    {
        playEncounters = 0;
        PlayerPrefs.SetInt($"{npcName}-PLAYER-ENCOUNTERS", playEncounters);
    }





    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            triggerCollider.enabled = false;
            playEncounters++;

            if (saveData)
            {
                PlayerPrefs.SetInt($"{npcName}-PLAYER-ENCOUNTERS", playEncounters);
            }

            if (CheckForReadyConversationEncounter(playEncounters, out EncounterEvent encounter))
            {
                PlayEncounter(encounter);
            }
        }
    }


    public void RotateToFaceTarget()
    {
        Transform playerTransform = SceneRegistry.Instance.player.transform;
        Vector3 direction = transform.parent.position - playerTransform.position;
        direction.y = 0f; // Keep rotation horizontal

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            playerTransform.DOKill(); // Stop any ongoing tweens
            playerTransform.DORotateQuaternion(targetRotation, 1).SetEase(Ease.Linear);
        }
    }



    public void PlayEncounter(EncounterEvent encounter)
    {
        if (encounter == null)
        {
            Debug.LogError($"NPC Conversation of encounter : {encounter.playConversationID} : could not be found", gameObject);
            return;
        }

        currentEncounter = encounter;
        PlayerInteractionsHandler.instance.dialogueCharacter = GetComponentInParent<NPCharacter>();
        DialogueManager.StartConversation(encounter.playConversationID, SceneRegistry.Instance.player.transform, transform);


        if (encounter.rotatePlayerCamera)
        {
            RotateToFaceTarget();
        }

    }


    EncounterEvent GetNPCConversation(int encounterCount)
    {
        for (int i = 0; i < conversations.Length; i++)
        {
            if (conversations[i].encounters == encounterCount)
            {
                return conversations[i];
            }
        }

        return null;
    }

    public bool CheckForReadyConversationEncounter(int encounterCount, out EncounterEvent encounter)
    {
        for (int i = 0; i < conversations.Length; i++)
        {
            if (conversations[i].encounters == encounterCount)
            {
                encounter = conversations[i];
                return true;
            }
        }
        encounter = null;
        return false;
    }
        
}
