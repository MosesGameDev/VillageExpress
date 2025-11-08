using UnityEngine;
using UnityEngine.Events;

public class AnimationEventHandler : MonoBehaviour
{
    [System.Serializable]
    public class AnimEvent
    {
        public string eventName;
        public UnityEvent unityEvent;
    }

    public AnimEvent[] animationEvents;
    private Animator animator;


    public void PauseAnimator()
    {
        if (animator != null)
        {
            animator.speed = 0f;
        }
    }
    public void ResumeAnimator()
    {
        if (animator != null)
        {
            animator.speed = 1f;
        }
    }

    public void TriggerEvent(string eventName)
    {
        foreach (var animEvent in animationEvents)
        {
            if (animEvent.eventName == eventName)
            {
                animEvent.unityEvent.Invoke();
                break;
            }
        }
    }
}
