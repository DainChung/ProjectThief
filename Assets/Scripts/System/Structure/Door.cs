using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{
    public class Door : ControlledStructure
    {
        private bool opend = false;
        private bool triggered = false;

        private float doorSpeed = 50f;

        public Transform doorMesh;

        void Start()
        {
            SetItem(ItemCode.STRUCTURE);
        }
        public override void UseStructure()
        {
            if (!triggered) StartCoroutine(OpenClose());
        }

        private IEnumerator OpenClose()
        {
            triggered = true;

            float y = (opend ? 90f : 0f);
            transform.parent.GetChild(0).GetComponent<BoxCollider>().enabled = false;
            transform.parent.GetChild(1).GetComponent<BoxCollider>().enabled = false;
            while (true)
            {
                if (opend)
                {
                    if (doorMesh.localEulerAngles.y >= 350f) break;
                    else doorMesh.localRotation = Quaternion.Euler(0, y, 0);
                    y -= Time.deltaTime * doorSpeed;
                }
                else
                {
                    if (doorMesh.localEulerAngles.y >= 90f) break;
                    else doorMesh.localRotation = Quaternion.Euler(0, y, 0);

                    y += Time.deltaTime * doorSpeed;
                }
                yield return null;
            }

            opend = !opend;
            doorMesh.localRotation = Quaternion.Euler(0, (opend ? 90f : 0), 0);
            transform.parent.GetChild(0).GetComponent<BoxCollider>().enabled = true;
            transform.parent.GetChild(1).GetComponent<BoxCollider>().enabled = true;
            triggered = false;
            yield break;
        }
    }
}
