using UnityEngine;
using UnityEngine.UI;

public class ClientStandByUI : MonoBehaviour
{
    public GameObject standByPanel;
    public Button btnStart;

    private PlayerStateManager localPlayer;

    void Start()
    {
        btnStart.onClick.AddListener(OnStartButtonClicked);
    }

    private void Update()
    {
        if (localPlayer == null)
        {
            FindMyPlayer();
            return;
        }

        // StandBy UI 활성화/비활성화
        if(localPlayer.CurrentState.Value == GameState.StandBy)
        {
            standByPanel.SetActive(true);
        }
        else
        {
            standByPanel.SetActive(false);
        }
    }

    void OnStartButtonClicked()
    {
        if (localPlayer != null)
        {
            localPlayer.RequestGameStart(); // 서버 시작 요청
        }
        else
        {
            Debug.LogWarning("아직 내 플레이어를 찾지 못했습니다!");
        }
    }

    void FindMyPlayer()
    {
        var player = FindObjectsByType<PlayerStateManager>(FindObjectsSortMode.None);
        foreach (var p in player)
        {
            if (p.IsOwner)
            {
                localPlayer = p;
                break;
            }
        }
    }
}
