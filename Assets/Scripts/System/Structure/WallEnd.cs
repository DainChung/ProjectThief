using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallEnd : MonoBehaviour
{
    public Transform nearWallEnd;
    public Transform wallEndToEnd;

    [HideInInspector]
    public Vector3 nearWallEndPos;
    [HideInInspector]
    public Vector3 wallEndToEndPos;

    void Start()
    {
        nearWallEndPos = nearWallEnd.position;
        wallEndToEndPos = wallEndToEnd.position;
    }
}
