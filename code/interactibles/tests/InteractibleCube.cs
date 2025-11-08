using DG.Tweening;
using EPOOutline;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InteractibleCube : Interactible
{
    Vector3 initPos;

    private void Start()
    {
        initPos = transform.position;
        OnStartInteraction += OnInteract;
        OnEndInteraction += OnInteractionEnd;
    }


    void OnInteract(Interactible interactible)
    {
        Vector3 pos = new Vector3 (transform.position.x, transform.position.y + .5f, transform.position.z);
        transform.DOJump(pos, 1, 2, .3f).OnComplete(() => { OnEndInteraction.Invoke(interactible); });
    }

    void OnInteractionEnd(Interactible interactible)
    {
        transform.DOKill();
        transform.position = initPos;
       
    }
}
