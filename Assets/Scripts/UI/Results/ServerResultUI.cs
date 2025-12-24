using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ServerResultUI : MonoBehaviour
{
    [System.Serializable]
    public class ResultPanel
    {
        public GameObject panelObj;
        public TMP_Text textName;
        public TMP_Text textScore;
    }

    // 왼쪽
    public ResultPanel leftResult;
    // 오른쪽
    public ResultPanel rightResult;

    // 코루틴 중복 방ㅈ
    private bool isLeftRoutineRunning = false;
    private bool isRightRoutineRuuing = false;

    // ServerGameUI의 연출 시간 1.5 이펙트 + 3초 팝업
    [Header("ServerGameUI의 연출시간동안 지연")]
    [SerializeField]
    private float delayTime = 4.5f;

    // Update is called once per frame
    void Update()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;

        foreach (var player in FindObjectsByType<PlayerStateManager>(FindObjectsSortMode.None))
        {
            if (player.OwnerClientId == NetworkManager.ServerClientId) continue;

            string role = player.MyRole.Value.ToString();

            bool isResultState = (player.CurrentState.Value == GameState.Result);

            if (role == "Left")
            {
                HandleResultPanel(leftResult,player,isResultState, true);
            }
            else if (role == "Right")
            {
                HandleResultPanel(rightResult,player,isResultState, false);
            }
        }
    }

    void HandleResultPanel(ResultPanel ui, PlayerStateManager player, bool isActive, bool isLeft)
    {
        // 데이터 갱신
        if (isActive)
        {
            bool isRunning = isLeft ? isLeftRoutineRunning : isRightRoutineRuuing;

            if (!ui.panelObj.activeSelf && !isRunning)
            {
                StartCoroutine(ShowPanelRoutine(ui, player, isLeft));
            }
            else if (ui.panelObj.activeSelf)
            {
                if (ui.textName != null)
                {
                    ui.textName.text = player.PlayerName.Value.ToString();
                }
                if (ui.textScore != null)
                {
                    ui.textScore.text = player.Score.Value.ToString();
                }
            }
        }
        else
        {
            if (ui.panelObj.activeSelf) ui.panelObj.SetActive(false);

            // 상태 초기화
            if (isLeft) isLeftRoutineRunning = false;
            else isRightRoutineRuuing = false;
        }
    }

    // 4.5초 기다리는 함수
    IEnumerator ShowPanelRoutine(ResultPanel ui, PlayerStateManager player, bool isLeft)
    {
        // 중복 실행 방지
        if (isLeft) isLeftRoutineRunning = true;
        else isRightRoutineRuuing = true;

        // 4.5초 대기
        yield return new WaitForSeconds(delayTime);

        // 이 후 결과 상태라면 패널 on
        if (player.CurrentState.Value == GameState.Result)
        {
            ui.panelObj.SetActive(true);

            // 켜지면 데이터 갱신
            if (ui.textName) ui.textName.text = player.PlayerName.Value.ToString();
            if (ui.textScore) ui.textScore.text = player.Score.Value.ToString();
        }

        // 작업 끝 중복 X
        if(isLeft) isLeftRoutineRunning = false;
        else isRightRoutineRuuing=false;
    }

}
