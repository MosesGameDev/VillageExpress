using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UIBlocker : MonoBehaviour
{
    CanvasGroup canvasGroup;

    private void Start()
    {
       canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Show()
    {
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

    }

    public void Hide()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

    }

}
