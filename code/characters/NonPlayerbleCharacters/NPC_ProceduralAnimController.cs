using FIMSpace.BonesStimulation;
using FIMSpace.FEyes;
using FIMSpace.FLook;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

public class NPC_ProceduralAnimController : MonoBehaviour
{
    [SerializeField] private FLookAnimator lookAnimator;
    [SerializeField] private FEyesAnimator eyesAnimator;

    public bool useLookAt = true;

    [Space]
    [SerializeField] private BonesStimulator[] bonesStimulators;

    [Space]
    [SerializeField] private float maxTriggerDistance = 10f;

    [Space]
    [SerializeField] private float viewConeAngle = 30f; // Half angle of the view cone
    [SerializeField] private float rotationSpeed = 5f; // Speed at which the NPC rotates to face player

    [Space]
    [SerializeField] private float fallout; // Value between 0-100
    private float quarterDistance; // One quarter of the trigger distance
    private float initialPlayerDistance; // Distance when player first enters trigger

    [SerializeField] private Vector3 initialLookRotation;
    Transform lookTarget;

    private void Start()
    {
        initialLookRotation = transform.parent.transform.eulerAngles;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            initialPlayerDistance = Vector3.Distance(transform.parent.position, other.transform.position);
            quarterDistance = maxTriggerDistance * 0.25f;
            fallout = 50f; // Start at middle value when entering trigger
            OnPlayerEnterTrigger();

            if(other.GetComponent<PlayerRefrences>())
            {
                lookTarget = other.GetComponent<PlayerRefrences>().lookAtTarget;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!useLookAt)
            {
                lookAnimator.LookAnimatorAmount = 0f;
                return;
            }
                // Handle rotation logic
                Vector3 directionToPlayer = other.transform.position - transform.parent.position;
            directionToPlayer.y = 0;

            Vector3 npcForward = transform.parent.forward;
            npcForward.y = 0;

            float angle = Vector3.Angle(npcForward, directionToPlayer);

            if (angle > viewConeAngle)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                transform.parent.rotation = Quaternion.Slerp(
                    transform.parent.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }

            // Handle fallout logic
            float currentDistance = Vector3.Distance(transform.parent.position, other.transform.position);
            float distanceChange = initialPlayerDistance - currentDistance;

            // Adjust fallout based on position relative to quarter distance
            if (Mathf.Abs(distanceChange) <= quarterDistance)
            {
                // Map the distance change to fallout value (0-100)
                float normalizedDistance = distanceChange / quarterDistance; // Will be between -1 and 1
                float falloutChange = normalizedDistance * 50f; // Convert to -50 to 50 range
                fallout = 50f + falloutChange; // Adjust around the middle value

                // Clamp the fallout value between 0 and 100
                fallout = Mathf.Clamp(fallout, 0f, 100f);
            }

            lookAnimator.LookAnimatorAmount = fallout / 100;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            fallout = 0f;
            transform.DOKill();
            ResetRotation();
            OnPlayerExitTrigger();
        }
    }

    void OnPlayerEnterTrigger()
    {
        EnableLookAnimator();
    }

    [Button]
    private void ResetRotation()
    {
        transform.parent.DOKill();
        transform.parent.DORotate(initialLookRotation, 1).SetDelay(1);
    }

    void OnPlayerExitTrigger()
    {
        DisableLookAnimator();
    }

    public bool isLookAnimatorEnabled()
    {
        return lookAnimator.enabled;
    }

    public float GetFallout()
    {
        return fallout;
    }

    public void EnableLookAnimator()
    {
        if (!lookAnimator.enabled)
        {
            lookAnimator.enabled = true;
        }

        if (!eyesAnimator.enabled)
        {
            eyesAnimator.enabled = true;
        }

        if (lookAnimator.EyesTarget == null)
        {
            if(lookTarget != null)
            {
                lookAnimator.SetLookTarget(lookTarget);
            }
        }

    }

    public void EnableLookAnimator(Transform target)
    {
        if (!lookAnimator.enabled)
        {
            lookAnimator.enabled = true;
        }

        if (!eyesAnimator.enabled)
        {
            eyesAnimator.enabled = true;
        }

        if (lookAnimator.EyesTarget == null)
        {
            lookAnimator.SetLookTarget(target);
            eyesAnimator.SetEyesTarget(target);
        }

    }

    public void EnableLookAnimator(Transform target, bool enableCollider)
    {
        if (!lookAnimator.enabled)
        {
            lookAnimator.enabled = true;
        }

        if (enableCollider)
        {
            GetComponent<Collider>().enabled = true;
        }

        if (!eyesAnimator.enabled)
        {
            eyesAnimator.enabled = true;
        }

        if (lookAnimator.EyesTarget == null)
        {
            lookAnimator.SetLookTarget(target);
        }

    }

    public void DisableLookAnimator()
    {
        if (lookAnimator.enabled)
        {
            lookAnimator.enabled = false;
        }


        if (eyesAnimator.enabled)
        {
            eyesAnimator.enabled = false;
        }


        lookAnimator.SetLookTarget(null);
    }

    public void DisableLookAnimator(bool disableCollider)
    {
        if (disableCollider)
        {
            GetComponent<Collider>().enabled = false;
        }

        if (lookAnimator.enabled)
        {
            lookAnimator.enabled = false;
        }


        if (eyesAnimator.enabled)
        {
            eyesAnimator.enabled = false;
        }


        lookAnimator.SetLookTarget(null);
    }

    public void EnableBoneSimulators()
    {
        for (int i = 0; i < bonesStimulators.Length; i++)
        {
            bonesStimulators[i].enabled = true;
        }
    }

    public void DisableBoneSimulators()
    {
        for (int i = 0; i < bonesStimulators.Length; i++)
        {
            bonesStimulators[i].enabled = false;
        }
    }


    public void EnableTrigger()
    {
        GetComponent<Collider>().enabled = true;
    }

    public void DisableTrigger()
    {
        GetComponent<Collider>().enabled = false;
    }

    public void DisableProcedurals()
    {
        DisableLookAnimator(true);
        DisableBoneSimulators();
        DisableTrigger();
    }

}
