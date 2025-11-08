using UnityEngine;
using UnityEngine.Events;

public class FoldingDoorAnimHandler : MonoBehaviour
{
    public UnityEvent onDoorOpen;
    public UnityEvent onDoorClose;

    public void OnDoorOpen()
    {
        onDoorOpen.Invoke();
    }

    public void OnDoorClose()
    {
        onDoorClose.Invoke();
    }
}
