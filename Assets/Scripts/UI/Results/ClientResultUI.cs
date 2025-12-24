using UnityEngine;
using UnityEngine.UI;

public class ClientResultUI : MonoBehaviour
{
    // UI
    public GameObject resultPanel;
    public Button btnNext;

    private PlayerStateManager localPlayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (btnNext != null)
        {
            btnNext.onClick.AddListener(OnNExtClicked);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (localPlayer == null)
        {
            FindMyPlayer();
            return;
        }

        bool isResultState = (localPlayer.CurrentState.Value == GameState.Result);

        if (resultPanel != null && resultPanel.activeSelf != isResultState)
        {
            resultPanel.SetActive(isResultState);
        }
    }

    void OnNExtClicked()
    {
        if (localPlayer != null)
        {
            Debug.Log("다음 단계 랭킹 요청");
            localPlayer.RequestRanking();
        }
    }

    void FindMyPlayer()
    {
        var players = FindObjectsByType<PlayerStateManager>(FindObjectsSortMode.None);
        foreach (var p in players)
        {
            if (p.IsOwner)
            {
                localPlayer = p;
                break;
            }
        }
    }
}
