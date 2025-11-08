using DG.Tweening;
using UnityEngine;

public class NPC_TriggerRotateToFacePlayer : MonoBehaviour
{
    [SerializeField] private Transform character;
    [Space]

    [SerializeField] float stopDistance;

    [SerializeField] private float minDegrees = -35f;
    [SerializeField] private float maxDegrees = 35f;
    [SerializeField] private float rotationSpeed = 5f;
    [Space]
    [SerializeField] private bool togglePlayerMovement;
    private Transform playerTransform;
    private bool isPlayerInTrigger = false;



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            stopDistance = GetComponent<SphereCollider>().radius / 2;
            playerTransform = other.transform;
            isPlayerInTrigger = true;
        }
    }


    private void OnTriggerStay(Collider other)
    {


        if (other.CompareTag("Player") && playerTransform != null)
        {
            if (!isPlayerInTrigger)
            {
                return;
            }

            Vector3 cp = new Vector3(character.position.x, 0, character.position.z);
            Vector3 pp = new Vector3(playerTransform.position.x, 0, playerTransform.position.z);

            Vector3 directionToPlayer = playerTransform.position - character.position;
            directionToPlayer.y = 0; // Ignore vertical differences
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

            float distance = Vector3.Distance(cp, pp);

            if (distance <= stopDistance)
            {
                DisableMovement();
                isPlayerInTrigger = false;

                Vector3 targetPos = character.position;
                targetPos.y = playerTransform.position.y; // Keep the same Y level
                playerTransform.DOLookAt(targetPos, 1);

                Vector3 tPos = playerTransform.position;
                tPos.y = character.position.y;
                character.DOLookAt(tPos, 1);
                return;
            }

            float angle = Vector3.SignedAngle(character.forward, directionToPlayer.normalized, Vector3.up);

            if (angle < minDegrees || angle > maxDegrees)
            {
                character.rotation = Quaternion.Slerp(character.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }



    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerTransform = null;
            isPlayerInTrigger = false;
            EnableMovement();
            character.DOKill();
        }
    }


    public void EnableMovement()
    {
        SceneRegistry.Instance.player.firstPersonController.playerCanMove = true;
        SceneRegistry.Instance.player.firstPersonController.cameraCanMove = true;
        SceneRegistry.Instance.player.firstPersonController.enableHeadBob = true;

        Cursor.lockState = CursorLockMode.Locked;

    }

    public void DisableMovement()
    {
        SceneRegistry.Instance.player.firstPersonController.playerCanMove = false;
        SceneRegistry.Instance.player.firstPersonController.cameraCanMove = false;
        SceneRegistry.Instance.player.firstPersonController.enableHeadBob = false;

        Cursor.lockState = CursorLockMode.Confined;
    }
}