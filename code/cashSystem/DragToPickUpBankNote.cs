using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragToPickUpBankNote : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int numberOfNotes;
    public int noteValue;

    [Space]
    [SerializeField] private BankNote notePrefab;
    [SerializeField] private CanvasGroup outline;

    [Space]
    public TextMeshProUGUI noteCountText;
    public Image noteCountTextImageBackground;
    public GameObject blocked;

    [Space]
    [SerializeField] RectTransform rectTransform;
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;
    private BankNote currentNote;

    private Vector2 dragOffset;
    private bool isHovered = false;
    private bool isTweening = false;

    public static event Action<int> OnPickUpNote;
    /// <summary>
    /// called when Note is dropped;
    /// int NoteValue, int count==1, bool [dropped on player?]
    /// </summary>
    public static event Action<int, int, bool> OnDropNote;


    #region Events
    private void OnEnable()
    {
        BankNoteCurrencyManager.OnAddCurrency += BankNoteCurrencyManager_OnAddCurrency;
    }

    private void BankNoteCurrencyManager_OnAddCurrency(int _noteValue, int count)
    {
        if (_noteValue == noteValue)
        {
            numberOfNotes = count;
            UpdateNoteCountText();
        }
    }

    private void OnDisable()
    {
        BankNoteCurrencyManager.OnAddCurrency -= BankNoteCurrencyManager_OnAddCurrency;
    }
    #endregion


    private void Start()
    {
        UpdateNoteCountText();
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        if (blocked.activeInHierarchy)
        {
            blocked.SetActive(false);
        }

        noteCountTextImageBackground.color = Color.yellow;


        currentNote = Instantiate(notePrefab, canvas.transform);
        currentNote.transform.rotation = Quaternion.identity;
        currentNote.transform.SetParent(transform.parent);
        currentNote.transform.SetAsLastSibling();

        currentNote.pickupSlot = this;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out dragOffset);

        OnPickUpNote?.Invoke(noteValue);
        numberOfNotes--;
        UpdateNoteCountText();
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (currentNote != null)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, eventData.position, eventData.pressEventCamera, out localPoint);
            currentNote.GetComponent<RectTransform>().anchoredPosition = localPoint - dragOffset;
        }
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        if (currentNote != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            int layerMask = LayerMask.GetMask("NPC");

            if (Physics.Raycast(ray, out hit))
            {
                if ((layerMask & (1 << hit.collider.gameObject.layer)) != 0)
                {
                    Debug.Log("[GOOD]");
                    currentNote.transform.DOScale(Vector3.zero, 0.3f)
                    .OnComplete
                    (
                        () =>
                        {
                            OnDropNote?.Invoke(currentNote.value, 1, true);
                            currentNote.SetActive(false);
                            currentNote = null;
                        }
                    );
                    return;
                }
            }
            ReturnNoteToStack();
        }

        ReturnNoteToStack();
    }


    void ReturnNoteToStack()
    {
        Sequence noteReturnSequence = DOTween.Sequence();

        noteReturnSequence
        .Append(currentNote.GetComponent<RectTransform>().DOAnchorPos(GetComponent<RectTransform>().anchoredPosition, 0.3f))
        .Insert(0, currentNote.transform.DOScale(Vector3.zero, noteReturnSequence.Duration()))
        .OnComplete
        (
            () =>
            {
                OnDropNote?.Invoke(currentNote.value, 1, false);
                currentNote.SetActive(false);
                currentNote = null;

                if (blocked.activeInHierarchy)
                {
                    blocked.SetActive(false);
                    canvasGroup.blocksRaycasts = true;
                    noteCountTextImageBackground.color = Color.yellow;
                }

                transform.DOPunchScale(Vector3.one * .1f, 0.3f).OnComplete(() => { transform.localScale = Vector3.one; });
            }
        );
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (numberOfNotes < 1 || isTweening)
        {
            return;
        }
        isHovered = true;
        isTweening = true;
        outline.alpha = 1;
        transform.DOScale(Vector3.one * 1.1f, 0.1f).OnComplete(() => isTweening = false);
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        if (isTweening)
        {
            return;
        }

        if (numberOfNotes < 1)
        {
            blocked.SetActive(true);
            noteCountTextImageBackground.DOColor(Color.red, .3f);
            canvasGroup.blocksRaycasts = false;
        }

        isHovered = false;
        outline.alpha = 0;
        transform.DOScale(Vector3.one, 0.1f).OnComplete(() => isTweening = false);
    }


    void UpdateNoteCountText()
    {
        noteCountText.SetText(numberOfNotes.ToString());
    }
}

