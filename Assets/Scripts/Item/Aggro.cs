using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;

public class Aggro : MonoBehaviour
{
    private WeaponCode _code;

    public WeaponCode code { get { return _code; } }

    public void SetCode(WeaponCode weaponCode)
    {
        _code = weaponCode;
    }

    void Start()
    {
        //소음 파일 찾아서(?) 재생

        //지속시간 짧음
        Destroy(gameObject, 2.0f);
    }
}
