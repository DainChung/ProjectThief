using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Diagnostics;

public class WeaponThrow : MonoBehaviour
{
    public Rigidbody rb;

    private const float throwPower = 13f;
    private Stopwatch sw = new Stopwatch();

    // Start is called before the first frame update
    void Start()
    {
        rb.AddForce(transform.forward * throwPower, ForceMode.Impulse);
        sw.Start();
    }

    //생성 5초 후 자동 파괴
    void FixedUpdate()
    {
        if (sw.Elapsed.Seconds >= 5)
            Destroy(gameObject);
    }
}
