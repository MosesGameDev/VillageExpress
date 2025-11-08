using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractible
{

    void Highlight();

    void DisableHighlight();

    void StartInteraction();

    void EndInteraction();

}
