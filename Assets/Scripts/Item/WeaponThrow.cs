using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;

public class WeaponThrow : Weapon
{
    private WeaponCode code;
    private const float throwPower = 12f;

    public float timeValue;

    public void SetCode(WeaponCode weaponCode)
    {
        code = weaponCode;
    }

    void Awake()
    {
        base.rb = transform.GetComponent<Rigidbody>();
        base.time = timeValue;
    }

    // Start is called before the first frame update
    void Start()
    {
        base.rb.AddForce(transform.forward * throwPower, ForceMode.Impulse);
        UnityEngine.Debug.Log("Code: " + code);
        //생성 몇 초 후 자동 파괴
        Destroy(gameObject, base.time);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Structure"))
        {
            GameObject obj = Instantiate(Resources.Load(FilePaths.weaponPath + "Aggro") as GameObject, transform.position, transform.rotation) as GameObject;
            obj.GetComponent<Aggro>().SetCode(code);
        }
    }
}
