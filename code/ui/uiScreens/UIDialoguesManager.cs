using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class UIDialoguesManager : MonoBehaviour
{
    public static UIDialoguesManager Instance;
    public List<UIDialogueElement> uIDialogues = new List<UIDialogueElement>();

    private void Awake()
    {
        Instance = this;
    }

    [Button("Assign UIDialoges")]
    public void GetSceneUIElements()
    {
        uIDialogues.Clear();

        foreach (UIDialogueElement element in FindObjectsByType<UIDialogueElement>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            uIDialogues.Add(element);
        }
    }

    public UIDialogueElement GetUIDialogue(string id)
    {
        for (int i = 0; i < uIDialogues.Count; i++)
        {
            if (uIDialogues[i].dialogueID == id)
            {
                return uIDialogues[i];
            }
        }

        return null;
    }
}
