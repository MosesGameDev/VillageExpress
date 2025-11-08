using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using NUnit.Framework;
public class WaypointUIE : MonoBehaviour
{
    [SerializeField] private RawImage iconImage;
    [SerializeField] private RectTransform textContent;
    [SerializeField] private TextMeshProUGUI text;

    bool isHighlighted;


    public void OnHighlight(Interactible interactible)
    {  
        if(interactible != null)
        {
            isHighlighted = true;

            textContent.SetActive(true);
        }
    }


    public void OnDisableHighlight(Interactible interactible)
    {
        isHighlighted = false;

        if (textContent.gameObject.activeInHierarchy)
        {
            textContent.SetActive(false);
        }
    }

}
