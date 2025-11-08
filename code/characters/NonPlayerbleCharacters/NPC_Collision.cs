using System.Collections;
using UnityEngine;


[RequireComponent(typeof(Collider))]
public class NPC_Collision : MonoBehaviour
{
    public bool isPrimaryCollider;
    [SerializeField] Interactible interactible;

    public Interactible GetInteractible()
    {
        return interactible;
    }

 
}
