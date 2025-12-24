using System.Collections;
using System.Globalization;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerStateManager : NetworkBehaviour
{
 // 현재 진행 상태
 public NetworkVariable<GameState> CurrentState = new NetworkVariable<GameState>(GameState.StandBy);

 // Right , Left 판단용 - 서버가 설정
 public NetworkVariable<FixedString32Bytes> MyRole = new NetworkVariable<FixedString32Bytes>("");

// 클라 -> 서버 상태 변경 요청
public void RequestGameStart()
{
    if (IsOwner)
    {
        SubmitStateServerRpc(GameState.EnterName); // 다음 단계 이름 입력으로 변경 요청

    }
}
    // 상태 변경 로직
[ServerRpc]
private void SubmitStateServerRpc(GameState newState)
{
        ApplyGameState(newState);

}
    private void ApplyGameState(GameState newState)
    {
        // 상대 변전 전 처리
        if (newState == GameState.Ranking)
        {
            // 랭킹에 진입할 때 내 점수를 공유
            if (RankingManager.Instance != null)
            {
                RankingManager.Instance.AddScore(PlayerName.Value.ToString(), Score.Value);
            }

                StartCoroutine(RankingTimerRoutine());
        }
        else if (newState == GameState.StandBy)
        {
            ResetPlayerState();
        }

        // 실제 상태 변경
        CurrentState.Value = newState;
    }


    IEnumerator RankingTimerRoutine()
    {
        yield return new WaitForSeconds(8f);

        // 8초뒤 랭킹화면일 때 강제 대기화면
        if (CurrentState.Value == GameState.Ranking)
        {
            ApplyGameState(GameState.StandBy);
        }
    }

// --------------------------------------------------------

// 플레이어 이름 설정
    public NetworkVariable<FixedString64Bytes> PlayerName = new NetworkVariable<FixedString64Bytes>("");




    // 클라 -> 서버 이름 변경 요청
    public void UpdateName(string newName)
    {
        if (IsOwner)
        {
            UpdateNameServerRpc(newName);
        }
    }

    // 이름 변경 서버 요청
    [ServerRpc]
    private void UpdateNameServerRpc(string newName)
    {
        PlayerName.Value = newName;
        Debug.Log($"Player name updated to: {newName}");
    }

    // 이름 입력 완료
    public void RequestNameConfirm()
    {
        if (IsOwner)
        {
            SubmitStateServerRpc(GameState.NameConfirm); // 다음 단계 이름 확인으로 변경 요청
        }
    }

    // ----------------------------
    // NameConfirm 파트

    // 이름 수정
    public void RequestNameEdit()
    {
        if (IsOwner)
        {
            SubmitStateServerRpc(GameState.EnterName); // 이름 입력 단계로 돌아가기
        }
    }

    // 다음 단계 : 월드 선택 요청
    public void RequestWorldSelect()
    {
        if (IsOwner)
        {
            SubmitStateServerRpc(GameState.SelectWorld); // 다음 단계 월드 선택으로 변경 요청
        }
    }

    // ----------------------------
    // SelectWorld 파트

    // 선택한 월드 ID ( 0: 우주, 1 : 해양, 2: 인체 )
    public NetworkVariable<int> SelectedWorldId = new NetworkVariable<int>(0);

    // 월드 선택 요청
    public void RequestSelectWorld(int worldId)
    {
        if (IsOwner)
        {
            SubmitWorldSelectServerRpc(worldId);
        }
    }

    [ServerRpc]
    private void SubmitWorldSelectServerRpc(int worldId)
    {
        // 월드 정보 저장
        SelectedWorldId.Value = worldId;
        Debug.Log($"[PlayerStateManager] Player {OwnerClientId} selected World {worldId}");

        // 다음 단계로 진행
        CurrentState.Value = GameState.Tutorial;

        // 8초 타이버 시작
        StartCoroutine(TutorialTimerRoutine());
        tutorialTimer = 0f; //타이머 초기화
    }

    IEnumerator TutorialTimerRoutine()
    {
        Debug.Log("[PlayerStateManager] Tutorial timer started for 8 seconds.");
        yield return new WaitForSeconds(8f);

        Debug.Log("[PlayerStateManager] Tutorial timer ended. Moving to Playing state.");
        CurrentState.Value = GameState.Playing;
    }

    // --------------------------------------
    // 게임 진행 파트


    // 게임 진행 설정 값
    private const float TUTORIAL_TIME = 8f;
    private const float GAME_TIME = 60f;
    private float tutorialTimer = 0f;

    // 플레이어 점수
    public NetworkVariable<int> Score = new NetworkVariable<int>(0);

    // 개인별 게임 진행 시간
    public NetworkVariable<float> PlayTime = new NetworkVariable<float>(0f);

    // 서버 업데이트 - 각자 타이머
    private void Update()
    {
        if (IsServer)
        {
            // 튜토리얼 상태
            if (CurrentState.Value == GameState.Tutorial)
            {
                tutorialTimer += Time.deltaTime;
                if (tutorialTimer >= TUTORIAL_TIME)
                {
                    Debug.Log($"[Server] {OwnerClientId}번 플레이어 게임 시작!");
                    CurrentState.Value = GameState.Playing;
                    PlayTime.Value = 0f; // 초기화
                }

            }
            else if (CurrentState.Value == GameState.Playing)
            {
                PlayTime.Value += Time.deltaTime;

                if (PlayTime.Value >= GAME_TIME)
                {
                    Debug.Log($"[Server] {OwnerClientId}번 플레이어 게임 종료!");
                    CurrentState.Value = GameState.Result;
                }
            }
        }
    }


    // 점수 획득 함수
    public void AddScore(int amount)
    {
        if (IsServer)
        {
            Score.Value += amount;
        }
    }


    // -----------------------------------
    // 결과 -> 랭킹 파트

    // 랭킹 요청
    public void RequestRanking()
    {
        if (IsOwner)
        {
            SubmitStateServerRpc(GameState.Ranking);
        }
    }

    // ----------------------------------4
    // 랭킹 -> 대기 복귀

    public void RequestStandBy()
    {
        if (IsOwner)
        {
            SubmitStateServerRpc(GameState.StandBy);
        }
    }

    // 플레이어 정보 리셋
    private void ResetPlayerState()
    {
        Score.Value = 0;
        PlayTime.Value = 0f;
        PlayerName.Value = "";
        SelectedWorldId.Value = 0;
    }



}




