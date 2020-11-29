# ProjectThief

## 1. 개요

쥐를 피해 반지를 훔치고 탈출하는 Unity 잠입 게임입니다.


## 2. 플레이 영상

[![Video Label](http://img.youtube.com/vi/KWLyVQ_91eM/0.jpg)](https://youtu.be/KWLyVQ_91eM?t=0s)

[유튜브 링크](https://youtu.be/KWLyVQ_91eM)

## 3. 주요 기능

### 3.1. 파일 시스템



### 3.2. UI

NGUI를 사용하여 만들었습니다.


  #### 3.2.1. 메인 화면

  > 시작 화면

  ![시작화면 (1)](https://user-images.githubusercontent.com/11573611/100541461-76cc3400-3287-11eb-949f-396ad4a71036.jpg)

             

  > 스테이지 선택 화면

  ![시작화면 스테이지 (2)](https://user-images.githubusercontent.com/11573611/100541469-83508c80-3287-11eb-81d5-79f30df29333.jpg)

   - 스테이지를 클리어 한 경우 클리어 시간과 점수가 표시됩니다.
   - 스테이지별 최고 점수는 BestRecord.data에 저장됩니다.
   - 스테이지 1을 클리어하면 스테이지 2가 열립니다.

       

  > 설정

  ![시작화면 설정](https://user-images.githubusercontent.com/11573611/100541475-89df0400-3287-11eb-829e-4e51f50e25f0.jpg)

   - 게임 음량을 변경할 수 있습니다.
   - 변경된 게임 음량은 SoundSetting.data에 저장됩니다.

       
-------------------------------------------------------------------------------------------

  #### 3.2.2. 게임 화면

  > 게임 UI

  ![그림1](https://user-images.githubusercontent.com/11573611/100541166-4be0e080-3285-11eb-858f-1b9ac6e330b4.png)

  > 인벤토리

  ![인벤토리](https://user-images.githubusercontent.com/11573611/100541129-1c31d880-3285-11eb-97bb-508ffe7411d3.jpg)


  > 게임 메뉴

  ![게임 메뉴](https://user-images.githubusercontent.com/11573611/100541128-1b994200-3285-11eb-85c4-e809e167a55c.jpg)

-------------------------------------------------------------------------------------------
  > 목표(Indicator)
  
  - 현재 목표가 어디에 있는지 화면에 표시됩니다.
  - 반지를 습득한 경우 탈출 지점을 표시합니다.
  
  
  > > Incator.cs
<pre>
<code>
    public class Indicator : UI
    {
        public Transform target = null;
        private Vector3 pos
        {
          get { return (target == null ? Collections.ValueCollections.initPos : target.position); }
        }

        private UIController uiController;

        void Start()
        {
            Transform canvas = GameObject.FindGameObjectWithTag("Canvas").transform;
            Transform uiCam = GameObject.Find("UICamera").transform;
            uiController = GetComponent<UIController>();
            base.InitMaxWMaxH(canvas.TransformVector(transform.position) / uiCam.localScale.x);
        }

        void Update()
        {
            if (pos != Collections.ValueCollections.initPos)
                base.Move(pos);
            else
                uiController.OnOffAll(false);
        }
    }
</code>
</pre>

  > > UI.cs의 Move함수
  
  <pre>
  <code>
    protected virtual void Move(Vector3 destiPos)
    {
        destiPos = Camera.main.WorldToViewportPoint(destiPos);

        //화면을 넘어가지 않도록 함
        moveX = 2 * (Mathf.Clamp(destiPos.x, 0.10f, 0.80f) - 0.5f);
        moveY = 2 * (Mathf.Clamp(destiPos.y, 0.15f, 0.85f) - 0.5f);

        //방향 오류 수정
        if (destiPos.z < 0)
            moveY = -0.7f;

        destiPos.Set(moveX * maxW, moveY * maxH, 0);
        transform.localPosition = destiPos;
    }
  </code>
  </pre>
  
-------------------------------------------------------------------------------------------
  > 투척 궤도
  
  
-------------------------------------------------------------------------------------------
  
  > HP바
  
  
-------------------------------------------------------------------------------------------
  #### 3.2.3. 결과 화면
  
  > 성공
  
  ![게임 결과창-클리어-최고기록 - 복사본](https://user-images.githubusercontent.com/11573611/100541991-dc6def80-328a-11eb-875d-a46c64d0f04b.PNG)
  
  - 반지를 갖고 탈출지점으로 가면 출력됩니다.
  - 클리어 시간과 점수가 표시됩니다.
  - 최고 점수를 달성하면 BestRecord.data에 저장하고 추가 효과를 출력합니다.
  - 최고 점수가 아닌 경우 기록만 출력합니다.
  
  > 실패
  
  ![게임 결과창-실패](https://user-images.githubusercontent.com/11573611/100541936-96b12700-328a-11eb-9474-41748bbc6fe6.jpg)
  
  - 플레이어의 HP가 모두 소진되면 출력됩니다.
  - 메인 화면으로 돌아가거나 재도전 할 수 있습니다.
  
  
-------------------------------------------------------------------------------------------
