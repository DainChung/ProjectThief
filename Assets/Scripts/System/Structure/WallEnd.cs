using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallEnd : MonoBehaviour
{
    public Transform nearWallEnd;

    [HideInInspector]
    public Vector3 nearWallEndPos;

    void Start()
    {
        nearWallEndPos = nearWallEnd.position;
    }
}
