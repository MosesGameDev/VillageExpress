using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PassengerBoardingHUD : MonoBehaviour
{
    public Image tauntFillImage;
    public CanvasGroup canvasGroup;

    public Passenger passenger { get; set; }

    NPC_HUDUIElement npc_HUDUIElement;


    private void OnEnable()
    {
        NPC_VehicleBoardingHandler.OnTauntValueUpdated += FillProgress;
    }

    public void OnDisable()
    {
        NPC_VehicleBoardingHandler.OnTauntValueUpdated -= FillProgress;
    }


    Tween tween;
    public void FillProgress(float v, Passenger p)
    {
        if (passenger != p) return;

        v = Mathf.Clamp01(v);


        Show();

        if (tween != null && tween.IsActive())
        {
            tween.Kill();
            tween = null;
        }

        tween = tauntFillImage
            .DOFillAmount(v, 0.25f)
            .OnComplete(() =>
            {
                if (Mathf.Approximately(tauntFillImage.fillAmount, 1f))
                {
                    Hide();
                }
            });

        transform.DOKill(true); // true means complete before killing
        transform.localScale = Vector3.one; // reset baseline
        var rt = (RectTransform)transform;
        rt.DOPunchScale(Vector3.one * 0.5f, 0.25f, 5, 1)
          .OnComplete(() => rt.localScale = Vector3.one);
    }

    public void Show()
    {
       if (canvasGroup.alpha != 1)
        {
            canvasGroup.DOFade(1f, 0.25f);
            Debug.Log("Show PassengerBoardingHUD");
        }
    }

    public void Hide()
    {
        if (canvasGroup.alpha != 0)
        {
            canvasGroup.DOFade(0f, 0.25f);
        }
    }



}
