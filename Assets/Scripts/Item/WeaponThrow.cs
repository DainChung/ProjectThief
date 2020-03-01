using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponThrow : Weapon
{
    public float timeValue;

    private const float throwPower = 12f;

    void Awake()
    {
        base.rb = transform.GetComponent<Rigidbody>();
        base.time = timeValue;
    }

    // Start is called before the first frame update
    void Start()
    {
        base.rb.AddForce(transform.forward * throwPower, ForceMode.Impulse);
        //생성 몇 초 후 자동 파괴
        Destroy(gameObject, base.time);
    }
}
