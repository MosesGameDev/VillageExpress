using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToSlot : MoveToTargetTransform
{
    public bool isActive;
    public bool isOccupied;
    [Space]
    public NPCharacter character;
    public Vector3 position;

}