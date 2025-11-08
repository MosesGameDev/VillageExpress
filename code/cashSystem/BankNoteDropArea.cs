using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropArea : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        RectTransform droppedObject = eventData.pointerDrag.GetComponent<RectTransform>();
        if (droppedObject != null)
        {
            droppedObject.SetParent(transform);
            //droppedObject.anchoredPosition = Vector2.zero;
            //droppedObject.transform.DOScale(Vector3.zero, 0.3f).OnComplete(() => { droppedObject.transform.gameObject.SetActive(false); });
        }
        ShrinkChildren();
    }

    void ShrinkChildren()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.activeInHierarchy)
            {
                transform.GetChild(i).transform.DOScale(Vector3.zero, 0.3f).OnComplete(() => { transform.GetChild(i).transform.gameObject.SetActive(false); });
            }
        }
    }

}
