using DG.Tweening;
using FIMSpace.FProceduralAnimation;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]

public class NPC_MovementController : MonoBehaviour
{
    [SerializeField] private float movementSpeed;
    [SerializeField] private float stoppingDistance = 2;

    [Space]
    [SerializeField] private MoveToTargetTransform moveTargetLocationTransform;

    float maxSpeed = 3;
    Animator animator;
    NPCharacter character;
    NavMeshAgent agent;
    LegsAnimator legsAnimator;


    private void Start()
    {
        animator = GetComponent<Animator>();
        character = GetComponent<NPCharacter>();
        agent = GetComponent<NavMeshAgent>();
        legsAnimator = GetComponent<LegsAnimator>();
    }

    private void Update()
    {
        switch (character.state)
        {
            case NPC_States.NPC_STATES.MOVING:

                if (agent.isStopped)
                {
                    agent.isStopped = false;
                }

                MoveToLocation(moveTargetLocationTransform);
                break;
        }


    }

    public NavMeshAgent GetNavAgent()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }
        return agent;
    }

    public Animator GetCharacterAnimator()
    {
        return animator;
    }

    #region AI Movement

    [Button("Editor Debug Move To ...")]
    public void TestMove(float speed = 2)
    {
        movementSpeed = speed;

        if (character.state == NPC_States.NPC_STATES.SITTING)
        {
            Sequence sequence = DOTween.Sequence();
            character.ExitSeat(out sequence);

            sequence.OnComplete
            (
                () =>
                {
                    if (!agent.enabled)
                    {
                        agent.enabled = true;
                    }
                    character.state = NPC_States.NPC_STATES.MOVING;
                }
            );
            return;
        }

        character.state = NPC_States.NPC_STATES.MOVING;
    }

    /// <summary>
    /// Public access Method to call character "MoveToLocation"
    /// </summary>
    /// <param name="target"></param>
    public void MoveToTargetLocation(MoveToTargetTransform target)
    {

        if (target == null)
        {
            return;
        }

        //Character stands up, AIAgent moves to destination location
        if (character.state == NPC_States.NPC_STATES.SITTING)
        {
            Sequence sequence = DOTween.Sequence();
            character.ExitSeat(out sequence);

            sequence.OnComplete
            (
                () =>
                {
                    if (!agent.enabled)
                    {
                        agent.enabled = true;
                    }

                    moveTargetLocationTransform = target;
                    character.state = NPC_States.NPC_STATES.MOVING;
                }
            );
            return;
        }

        moveTargetLocationTransform = target;
        character.state = NPC_States.NPC_STATES.MOVING;
    }


    /// <summary>
    /// Moves Character to a new position (MoveToTargetTransform.transform.position), Action<Character> is called on Move to location
    /// </summary>
    /// <param name="target"></param>
    void MoveToLocation(MoveToTargetTransform target)
    {
        if (target == null)
        {
            return;
        }

        Vector3 characterPosition = new Vector3(transform.position.x, transform.position.z);
        Vector3 targetPosition = new Vector3(target.transform.position.x, target.transform.position.z);

        float distanceToTarget = Vector2.Distance(characterPosition, targetPosition);

        if (distanceToTarget <= agent.stoppingDistance)
        {
            character.state = NPC_States.NPC_STATES.IDLE;
            agent.isStopped = true;
            AnimatorSetSpeed(0);
            moveTargetLocationTransform = null;
            target.onCharacterReachLocation?.Invoke(character);
            return;
        }

        float speed = movementSpeed / maxSpeed;
        AnimatorSetSpeed(speed);

        agent.stoppingDistance = stoppingDistance;
        agent.speed = movementSpeed;
        agent.SetDestination(target.transform.position);
    }


    /// <summary>
    /// Set "speed" float value on Character animator, 0 == StopMoving, Sets Legs Animator Component Glueing Mode 0 = idle, .5>x = Moving
    /// </summary>
    /// <param name="speed"></param>
    public void AnimatorSetSpeed(float speed)
    {
        animator.SetFloat("speed", speed);

        if (legsAnimator == null)
        {
            Debug.LogError("Legs Animator missing:");
            return;
        }

        if (speed > 0)
        {
            if (legsAnimator.GlueMode != LegsAnimator.EGlueMode.Moving)
            {
                legsAnimator.GlueMode = LegsAnimator.EGlueMode.Moving;
            }
        }
        else
        {
            legsAnimator.GlueMode = LegsAnimator.EGlueMode.Idle;
        }
    }

    #endregion

}
