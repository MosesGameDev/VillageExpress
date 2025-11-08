using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class UIInteractionInstruction : MonoBehaviour
{
    [SerializeField] private Image intructionsImage;
    [SerializeField] private TextMeshProUGUI instructionText;
    private CanvasGroup canvasGroup;

    private void OnEnable()
    {
        PlayerInteractionsHandler.OnInteractibleHighlighted += PlayerInteractionsHandler_OnInteractibleHighlighted;
        PlayerInteractionsHandler.OnInteractibleClearHighlight += PlayerInteractionsHandler_OnInteractibleClearHighlight;
        PlayerInteractionsHandler.OnPlayerStartInteraction += PlayerInteractionsHandler_OnPlayerStartInteraction; ;
    }

    private void PlayerInteractionsHandler_OnPlayerStartInteraction(Interactible interactible)
    {

        if(!interactible.displayUIInstruction)
        {
            return;
        }

        Sequence sequence = DOTween.Sequence();

        sequence
        .Append(transform.DOPunchScale(Vector3.one * 0.15f, .2f))
        .Append(canvasGroup.DOFade(0, sequence.Duration()))
        .OnComplete(() => { transform.localScale = Vector3.zero; });
    }


    #region Events
    private void PlayerInteractionsHandler_OnInteractibleClearHighlight(Interactible interactible)
    {
        canvasGroup.DOFade(0, .15f);
    }

    private void PlayerInteractionsHandler_OnInteractibleHighlighted(Interactible interactible)
    {
        if (!interactible.displayUIInstruction)
        {
            return;
        }

        transform.localScale = Vector3.one;
        canvasGroup.DOFade(1, .15f);

        if (interactible.instructionSprite != null)
        {
            intructionsImage.sprite = interactible.instructionSprite;
        }

        instructionText.color = interactible.instructionsTextColor;
        instructionText.SetText(interactible.instruction);

    }
    #endregion

    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
    }



    private void OnDisable()
    {
        PlayerInteractionsHandler.OnInteractibleHighlighted -= PlayerInteractionsHandler_OnInteractibleHighlighted;
        PlayerInteractionsHandler.OnInteractibleClearHighlight -= PlayerInteractionsHandler_OnInteractibleClearHighlight;
        PlayerInteractionsHandler.OnPlayerStartInteraction -= PlayerInteractionsHandler_OnPlayerStartInteraction; ;
    }

}
