using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ServerGameUI : MonoBehaviour
{
    [System.Serializable]
    public class PlayerUI
    {
        // 전체 패널
        public GameObject gamePanel;
        // 팝업
        public GameObject startMentPopup;
        public GameObject endMentPopup;

        // 상단 UI
        public Image imgWorldTitle;
        public Image imgTimeGauge;
        public TMP_Text textTotalScore;


        public bool isRunning;
        public bool isFinished;
    }

    // 왼쪽 플레이어 UI
    public PlayerUI leftUI;
    // 오른쪽 플레이어 UI
    public PlayerUI rightUI;

    // 설정
    public float maxGameTime = 60f;

    // 월드 이름 데이터
    public Sprite[] worldTitleSprites;

    // 배경 매니저 연결
    public BackgroundManager backgroundManager;

    private void Update()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;

        // 접속여부 체크
        bool isLeftPresent = false;
        bool isRightPresent = false;

        foreach (var player in FindObjectsByType<PlayerStateManager>(FindObjectsSortMode.None))
        {
            string role = player.MyRole.Value.ToString();

            var currentState = player.CurrentState.Value;
            bool isPlaying = (player.CurrentState.Value == GameState.Playing);
            bool isResult = (player.CurrentState.Value == GameState.Result);
            bool isStandBy = (currentState == GameState.StandBy); // 대기 상태 확인

            // 대기상태시 초기화
            if (isStandBy)
            {
                if (role == "Left") ResetUI(leftUI);
                else if (role == "Right") ResetUI(rightUI);
                continue;
            }

            if (role == "Left")
            {
                isLeftPresent = true;
                UpdatePlayerUI(leftUI, player, isPlaying, isResult, "Left");
            }
            else if (role == "Right")
            {
                isRightPresent = true;
                UpdatePlayerUI(rightUI, player, isPlaying, isResult, "Right");
            }
        }

        if (!isLeftPresent) ResetUI(leftUI);
        if (!isRightPresent) ResetUI(rightUI);
       
    }

    // 초기화
    void ResetUI(PlayerUI uI)
    {
        uI.isRunning = false;
        uI.isFinished = false;

        // 켜져있는 패널 끄기
        if(uI.gamePanel && uI.gamePanel.activeSelf) uI.gamePanel.SetActive(false);
        if(uI.startMentPopup && uI.startMentPopup.activeSelf) uI.startMentPopup.SetActive(false);
        if(uI.endMentPopup && uI.endMentPopup.activeSelf) uI.endMentPopup.SetActive(false) ;
    }

    void UpdatePlayerUI(PlayerUI ui, PlayerStateManager player, bool isPlaying, bool isResult, string role)
    {
        // 게임 종료 처리
        if (isResult)
        {
            if (!ui.isFinished)
            {
                ui.isFinished = true;

                // 배경 매니저 연결 요청
                if(backgroundManager != null) backgroundManager.PlayEndEffect(role);
                StartCoroutine(EndGameRoutine(ui));
            }
        }


        // playing 아니면 패널 끄기
        if (!isPlaying) return;

        // Playing 상태 진입 - 최초1회 실행
        if (!ui.isRunning)
        {
            ui.isRunning = true;
            ui.isFinished = false;
            ui.gamePanel.SetActive(true);

            // 월드 이름 셋팅
            int wId = player.SelectedWorldId.Value;
            if (ui.imgWorldTitle != null && worldTitleSprites != null && wId < worldTitleSprites.Length)
            {
                ui.imgWorldTitle.sprite = worldTitleSprites[wId];
            }

            // 팝업 뛰우기 2초
            if (ui.startMentPopup)
            {
                ui.startMentPopup.SetActive(true);
                StartCoroutine(HidePopupRoutine(ui.startMentPopup));
            }
        }


        // 실시간 정보 갱신
        if (ui.textTotalScore != null) ui.textTotalScore.text = $"{player.Score.Value}";

        // 시간 게이지 연동
        if (ui.imgTimeGauge != null)
        {
            float currentPlayTime = player.PlayTime.Value;

            // 0~1 차오르는 애니메이션
            if (currentPlayTime < 2.0f)
            {
                ui.imgTimeGauge.fillAmount = currentPlayTime / 2.0f;
            }
            // 2초 이후 실제 남은 시간에 맞춤
            else
            {
                float remainTime = Mathf.Max(0, maxGameTime - currentPlayTime);
                ui.imgTimeGauge.fillAmount = remainTime / maxGameTime;
            }
        }

    }
   
    // 종료
    IEnumerator EndGameRoutine(PlayerUI uI)
    {
        Debug.Log("게임 종료: 팝업 출력");
        yield return new WaitForSeconds(1.5f);  // 이펙트 1.5초 대기

        if(uI.endMentPopup != null) uI.endMentPopup.SetActive(true);

        
        yield return new WaitForSeconds(3.0f);

        // 맵 퇴장
        if (uI.endMentPopup != null) uI.endMentPopup.SetActive(false);
        if(uI.gamePanel != null) uI.gamePanel.SetActive(false);

        uI.isRunning = false;
    }


    IEnumerator HidePopupRoutine(GameObject popup)
    {
        yield return new WaitForSeconds(2f);
        if (popup) popup.SetActive(false);
    }

}
