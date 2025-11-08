using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Acts as target location marker for NPCharacter AIAgent
/// </summary>
public class MoveToTargetTransform : MonoBehaviour
{
    public string locationId = "location";
    public System.Action<NPCharacter> onCharacterReachLocation;

}
