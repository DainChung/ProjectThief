using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MyCompany.MyGame.GameSystem
{
    public class SetStage : MonoBehaviour
    {
        private const float floorD = 3f;

        public Vector2 floorSize = Vector2.zero;
        public string floorName;
        public string wallName;

        void Start()
        {
            Transform floor = transform.Find("Floor");
            Transform wall = transform.Find("Wall");
            float h = floor.position.y + 0.55f;
            Vector3 v = Vector3.zero;

            string floorPath = string.Format("Structure/{0}", floorName);
            string wallPath = string.Format("Structure/{0}", wallName);
            for (int x = 0; x < floorSize.x; x++)
            {
                for (int y = 0; y < floorSize.y; y++)
                {
                    v.Set(floor.position.x + x * floorD, h, floor.position.y + y * floorD);
                    Instantiate(Resources.Load(floorPath), v, floor.rotation, floor);
                    if (x == 0)
                    {
                        v.Set(v.x, h - 0.05f, v.y);
                        Instantiate(Resources.Load(wallPath), v, wall.rotation, wall);
                    }
                    else if (y == 0)
                    {
                        v.Set(v.x, h - 0.05f, v.y);
                        Instantiate(Resources.Load(wallPath), v, wall.rotation, wall);
                    }
                }
            }
        }
    }
}
