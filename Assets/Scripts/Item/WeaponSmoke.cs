using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSmoke : Weapon
{
    public float timeValue;

    void Awake()
    {
        base.time = timeValue;
    }

    void Start()
    {
        //생성 몇 초 후 자동 파괴
        Destroy(gameObject, base.time);
    }
}
