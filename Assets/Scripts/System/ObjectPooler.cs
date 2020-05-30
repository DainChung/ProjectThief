using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{
    public class ObjectPooler : MonoBehaviour
    {
        public static ObjectPooler sharedInstance;

        public Dictionary<string, List<GameObject>> poolObjs;
        public GameObject[] obj = new GameObject[2];
        public int[] poolAmount = new int[2];

        void Awake()
        {
            poolObjs = new Dictionary<string, List<GameObject>>();
            sharedInstance = this;
        }

        void Start()
        {
            for (int objIndex = 0; objIndex < obj.Length; objIndex++)
            {
                List<GameObject> objList = new List<GameObject>();
                for (int i = 0; i < poolAmount[objIndex]; i++)
                {
                    GameObject o = Instantiate(obj[objIndex]) as GameObject;
                    try
                    {
                        o.GetComponent<WeaponThrow>().Init();
                    }
                    catch (System.Exception e) { Debug.Log(transform.name + "("+this.name+")"+ " : " + e); }
                    o.SetActive(false);
                    objList.Add(o);
                }
                poolObjs.Add(obj[objIndex].name, objList);
            }
        }

        private GameObject GetPooledObj(string name)
        {
            List<GameObject> list = poolObjs[name];
            for (int i = 0; i < list.Count; i++)
            {
                if (!list[i].activeInHierarchy)
                    return list[i];
            }

            return null;
        }

        public void PoolObj(Vector3 pos, Quaternion rot, string name)
        {
            GameObject o = GetPooledObj(name);

            if (o != null)
            {
                o.transform.position = pos;
                o.transform.rotation = rot;
                o.SetActive(true);
                try
                {
                    WeaponThrow weapon = o.GetComponent<WeaponThrow>();
                    StartCoroutine(weapon.Disposer());
                    weapon.AddForce();
                }
                catch (System.Exception){ }
            }
        }
    }
}
