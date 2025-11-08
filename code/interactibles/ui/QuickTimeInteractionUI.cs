using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class QuickTimeInteractionUI : MonoBehaviour
{
    [SerializeField] private float duration = 1f;
    [SerializeField] private float range = .1f;
    [Space]
    [SerializeField] private TextMeshProUGUI responseText;
    [SerializeField] private TextMeshProUGUI passengerFareText;
    [Space]
    [SerializeField] private RectTransform arrowRect;
    [SerializeField] private RectTransform selectionRangeRect;
    [Space]
    [SerializeField] private CanvasGroup canvasGroup;

    /// <summary>
    /// Returns True if QTE stops in green area
    /// </summary>
    public Action<bool> OnStop;

    Vector2 range2D;
    Tween arrowTween;
    float arrowValue;
    Passenger _interactingPassenger;


    [Button]
    public void PlayQuickTimeInteraction(Passenger passenger)
    {
        _interactingPassenger = passenger;
        responseText.SetText(string.Empty);
        passengerFareText.SetText(string.Empty);
        ShowCanvasGroup();
        SetSelectionRange();
        AnimateArrow();
    }


    public void StopQuicktime()
    {
        arrowTween.Pause();

        if (arrowValue >= range2D.x && arrowValue <= range2D.y)
        {
            responseText.SetText("Good!");
            OnStop?.Invoke(true);
        }
        else
        {
            responseText.SetText("Miss!");
            _interactingPassenger.commute.fareCost = reducedFare;
            OnStop?.Invoke(false);
        }

        responseText.rectTransform.DOPunchScale(Vector3.one * 0.1f, .3f);
        HideCanvasGroup();
    }


    public void ShowCanvasGroup()
    {
        canvasGroup.alpha = 1f;
    }


    public void HideCanvasGroup()
    {
        canvasGroup.DOFade(0, 0.1f).SetDelay(.35f);
        _interactingPassenger = null;
    }


    void SetSelectionRange()
    {
        float min = UnityEngine.Random.Range(0f, 1f);
        float max = UnityEngine.Random.Range(0f, 1f);

        if (min > max)
        {
            max = .5f;
            min = .4f;
        }

        if ((max - min) < range)
        {
            max += range;
        }

        selectionRangeRect.anchorMin = new Vector2(min, 0.5f);
        selectionRangeRect.anchorMax = new Vector2(max, 0.5f);

        range2D = new Vector2(min, max);
    }

    void AnimateArrow()
    {
        if (arrowTween == null)
        {
            PlayArrowTween();
            return;
        }

        arrowTween.Play();
    }


    /// <summary>
    /// passenger 'fareCost' is == 'reducedFare' if player misses green zone
    /// </summary>
    int reducedFare;
    void PlayArrowTween()
    {
        float value = 0;

        reducedFare = _interactingPassenger.commute.fareCost - UnityEngine.Random.Range(10, 20);
        reducedFare = Mathf.RoundToInt(reducedFare / 10) * 10;

        arrowTween = DOTween.To(() => value, x => value = x, 1, duration).SetLoops(-1, LoopType.Yoyo);
        arrowTween
        .OnUpdate
        (
            () =>
            {
                arrowRect.anchorMin = new Vector2(value, arrowRect.anchorMin.y);
                arrowRect.anchorMax = new Vector2(value, arrowRect.anchorMax.y);

                arrowRect.anchoredPosition = new Vector2(value, arrowRect.anchoredPosition.y);
                arrowValue = value;

                if (arrowValue >= range2D.x && arrowValue <= range2D.y)
                {
                    passengerFareText.SetText($"Bus fare: KES <b>{_interactingPassenger.commute.fareCost}</b> /=");
                }
                else
                {
                    passengerFareText.SetText($"Bus fare: KES <b>{reducedFare}</b> /=");
                }
            }
        );

    }
}
