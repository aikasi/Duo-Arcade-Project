using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

public class BackgroundManager : MonoBehaviour
{
    // 배경
    [System.Serializable]
    public class PlayerBG
    {
        [Header("제일 뒤 검은색 배경 -> 동영상나올 때 비활성화")]
        public GameObject background;
        public GameObject defaultBackground;

        public GameObject swirlEffectObject;
        public VideoPlayer gameVideoPlayer;
        public bool isChanged;
    }

    // 왼쪽 / 오른쪽 배경
    public PlayerBG leftSet;
    public PlayerBG rightSet;

    // 월드 배경들
    public VideoClip[] worldVideoClip;


 
    void Update()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;

        // 플레이어 중 한 명이라도 Playing 상태면 시작
        foreach (var player in FindObjectsByType<PlayerStateManager>(FindObjectsSortMode.None))
            {
                string role = player.MyRole.Value.ToString();
                bool isPlaying = (player.CurrentState.Value == GameState.Playing);
                bool isStandBy = (player.CurrentState.Value == GameState.StandBy);
            
                // 대기 상태 변수 초기화
                if (isStandBy)
            {
                if (role == "Left") leftSet.isChanged = false;
                else if (role == "Right") rightSet.isChanged = false;
                continue;
            }

                if (role == "Left")
                {
                    if (isPlaying && !leftSet.isChanged)
                    {
                        StartCoroutine(ChangeRoutine(leftSet, player.SelectedWorldId.Value));
                    }
                    else if (!isPlaying && leftSet.isChanged)
                    {
                    // 결과 때 필요하면 배경 초기화 가능
                    leftSet.background.SetActive(true);
                    }
                }
                else if (role == "Right")
                {
                    if (isPlaying && !rightSet.isChanged)
                    {
                        StartCoroutine(ChangeRoutine(rightSet, player.SelectedWorldId.Value));
                    }
                    else if (!isPlaying && rightSet.isChanged)
                    {
                        rightSet.background.SetActive(true);
                    }
                }
            }
        
    }

    IEnumerator ChangeRoutine(PlayerBG bgSet, int worldId)
    {
        bgSet.isChanged = true;

        // 이펙트 활성화
        if(bgSet.swirlEffectObject != null) bgSet.swirlEffectObject.SetActive(true);

        // 비디오 준비
        if (bgSet.gameVideoPlayer && worldId < worldVideoClip.Length)
        {
            bgSet.gameVideoPlayer.clip = worldVideoClip[worldId];
            bgSet.gameVideoPlayer.isLooping = true;
            bgSet.gameVideoPlayer.Prepare();
        }

        yield return new WaitForSeconds(1.5f);

        // 배경   변경
        if(bgSet.defaultBackground != null) bgSet.defaultBackground.SetActive(false);

        // 비디오 재생
        if (bgSet.gameVideoPlayer)
        {
            bgSet.background.SetActive(false);
            bgSet.gameVideoPlayer.gameObject.SetActive(true);
            bgSet.gameVideoPlayer.Play();
        }

        // 소용돌이 끄기
        yield return new WaitForSeconds(0.5f);
        if(bgSet.swirlEffectObject != null) bgSet.swirlEffectObject.SetActive(false);
    }


    // 게임 종료시 호출할 함수
    public void PlayEndEffect(string role)
    {
        if (role == "Left") StartCoroutine(ResetRoutine(leftSet));
        else if (role =="Right") StartCoroutine(ResetRoutine(rightSet));
    }

    IEnumerator ResetRoutine(PlayerBG bgSet)
    {
        // 이펙트 시작
        if (bgSet.swirlEffectObject) bgSet.swirlEffectObject.SetActive(true);
        yield return new WaitForSeconds(1.5f);

        // 비디오 연결 제거
        if (bgSet.gameVideoPlayer)
        {
            bgSet.gameVideoPlayer.Stop();
            bgSet.gameVideoPlayer.gameObject.SetActive(false);
        }

        // 배경 복구
        if(bgSet.defaultBackground) bgSet.defaultBackground.SetActive(true);

        // 이펙트 종료
        yield return new WaitForSeconds(0.5f) ;
        if (bgSet.swirlEffectObject) bgSet.swirlEffectObject.SetActive(false);

        bgSet.isChanged = false; // 상태 초기화
    }
}
