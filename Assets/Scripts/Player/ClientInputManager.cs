using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClientInputManager : MonoBehaviour
{
    public GameObject gameControlPanel;

    // UI 연결
    public Button fireButton;
    public GameObject leftInputObj;
    public GameObject rightInputObj;

    // 스프라이트 바뀔 이미지
    public Image leftVisualImage;     // 실제로 이미지가 바뀔 대상
    public Image rightVisualImage;    // 실제로 이미지가 바뀔 대상

    // 이미지 리소스
    public Sprite imgFireNormal;    // 발사 - 평소
    public Sprite imgFireCooldown;  // 발사 - 쿨타임 중

    public Sprite imgLeftNormal;    // 왼쪽 - 평소
    public Sprite imgLeftPressed;   // 왼쪽 - 눌림

    public Sprite imgRightNormal;   // 오른쪽 - 평소
    public Sprite imgRightPressed;  // 오른쪽 - 눌림

    // 내부 변수 (이미지 캐싱)
    private Image imgCompFire;

    // 상태 변수
    private bool isLeftPressed = false;
    private bool isRightPressed = false;
    private bool isFireReady = true;

    // 로컬 플레이어
    private PlayerGunController localPlayer;
    private PlayerStateManager playerState;

    //결과 화면 지연 처리
    private const float RESULT_DELAY_TIME = 4.5f; // ClientResultUI와 같은 시간
    private bool isResultDelayActive = false;     // 4.5초 동안 UI 유지
    private GameState lastState = GameState.StandBy; 

    void Start()
    { 
        if (fireButton) imgCompFire = fireButton.GetComponent<Image>();
        if (imgCompFire && imgFireNormal) imgCompFire.sprite = imgFireNormal;
        if (fireButton) fireButton.onClick.AddListener(OnFireClicked);

        // 초기 이미지 설정
        if (leftVisualImage && imgLeftNormal) 
            leftVisualImage.sprite = imgLeftNormal;

        if (leftInputObj)
        {
            OnHoldPressClicked(leftInputObj, (isPressed) =>
            {
                isLeftPressed = isPressed;
                UpdateBtnVisual(leftVisualImage, isPressed ? imgLeftPressed : imgLeftNormal);
            });
        }


        if (rightVisualImage && imgRightNormal)
            rightVisualImage.sprite = imgRightNormal;

        if (rightInputObj)
        {
            OnHoldPressClicked(rightInputObj, (isPressed) => {
                isRightPressed = isPressed;
                UpdateBtnVisual(rightVisualImage, isPressed ? imgRightPressed : imgRightNormal);
            });
        }

    }

    void UpdateBtnVisual(Image targetImg, Sprite sprite)
    {
        if (targetImg != null && sprite != null)
        {
            targetImg.sprite = sprite;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (localPlayer == null || playerState == null)
        {
            if (gameControlPanel != null && gameControlPanel.activeSelf)
            {
                gameControlPanel.SetActive(false);
            }

            FindMyPlayer();
            return;
        }

        GameState currentState = playerState.CurrentState.Value;
        
        if (currentState != lastState)
        {
            if (currentState == GameState.Result)
            {
                StartCoroutine(KeepControlsRoutine());
            }
            else
            {
                isResultDelayActive = false;
            }
            lastState = currentState;
        }

        bool shouldShow = (currentState == GameState.Tutorial || currentState == GameState.Playing)
                           || (currentState == GameState.Result && isResultDelayActive);
        
        if (gameControlPanel != null && gameControlPanel.activeSelf != shouldShow)
        {
            gameControlPanel.SetActive(shouldShow);
        }

        if(!shouldShow)
        {
            return;
        }

        // 버튼 상태에 따른 플레이어 회전 처리
        if (isLeftPressed)
        {
            // 왼쪽 회전
            localPlayer.RequestRotate(-1f);
        }
        else if(isRightPressed)
        {
            // 오른쪽 회전
            localPlayer.RequestRotate(1f);
        }
    }

    // 발사 버튼 클릭 이벤트
    void OnFireClicked()
    {
        // 쿨타임 시 무시
        if (!isFireReady) return;

        if(localPlayer != null)
        {
            // 로컬 플레이어의 총 발사 메서드 호출
            localPlayer.RequestFire();

            //쿨타임
            StartCoroutine(FireCooldownRoutine());
        }
    }

    // 4.5초간 조작 UI 유지 
    IEnumerator KeepControlsRoutine()
    {
        isResultDelayActive = true;
        yield return new WaitForSeconds(RESULT_DELAY_TIME);
        isResultDelayActive = false;
    }

    // 0.5초 쿨타임
    IEnumerator FireCooldownRoutine()
    {
        isFireReady = false;

        UpdateBtnVisual(imgCompFire, imgFireCooldown);

        yield return new WaitForSeconds(0.5f);

        UpdateBtnVisual(imgCompFire, imgFireNormal);

        isFireReady=true;
    }

    // 플레이어를 찾는 메서드
    void FindMyPlayer()
    {
        var players = FindObjectsByType<PlayerGunController>(FindObjectsSortMode.None);
        foreach(var player in players)
        {
            if(player.IsOwner) 
            {
                localPlayer = player;
                playerState = player.GetComponent<PlayerStateManager>();
                break;
            }
        }
    }

    // OnTrigger를 통해 하는 방식 또한 가능
    // Up, Down 이벤트
    void OnHoldPressClicked(GameObject btn, System.Action<bool> action)
    {
        EventTrigger trigger = btn.GetComponent<EventTrigger>();
        if(trigger == null)
        {
            trigger = btn.AddComponent<EventTrigger>();
        }

        // Down
        EventTrigger.Entry entryDown = new EventTrigger.Entry();
        entryDown.eventID = EventTriggerType.PointerDown;
        entryDown.callback.AddListener((data) => { action(true); });
        trigger.triggers.Add(entryDown);

        // UP
        EventTrigger.Entry entryUp = new EventTrigger.Entry();
        entryUp.eventID = EventTriggerType.PointerUp;
        entryUp.callback.AddListener((data) => { action(false); });
        trigger.triggers.Add(entryUp);
    }

}
