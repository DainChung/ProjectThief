﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Com.MyCompany.MyGame
{
    /// <summary>
    /// 출발지점, 탈출지점 관리
    /// 점수 관리
    /// 승패 판정
    /// 일시정지
    /// 메뉴를 통한 종료 등의 관리
    /// </summary>
    public class StageManager : MonoBehaviour
    {
        #region Private Fields

        private GameObject player;
        private GameObject treasure;

        #endregion

        #region Public Fields

        public Transform start;
        public Transform[] end;

        #endregion



        // Start is called before the first frame update
        void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player");
            //treasure = GameObject.FindGameObjectWithTag("Treasure");

            //플레이어 캐릭터를 시작 지점으로 옮김
            player.transform.position = start.position;

            //탈출 지점을 숨김
            foreach (Transform endArea in end)
            {
                //endArea.GetComponent<MeshRenderer>().enabled = false;
                //endArea.GetComponent<BoxCollider>().enabled = false;
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
