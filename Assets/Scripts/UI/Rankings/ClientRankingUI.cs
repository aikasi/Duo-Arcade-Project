using UnityEngine;
using UnityEngine.UI;

public class ClientRankingUI : MonoBehaviour
{
    public GameObject rankingPanel;
    public Button btnGoToStanBy;

    private PlayerStateManager localPlayer;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        btnGoToStanBy.onClick.AddListener(OnBtnClick);
    }

    // Update is called once per frame
    void Update()
    {
        if (localPlayer == null)
        {
            FindMyPlayer();
            return;
        }

        bool isRanking = (localPlayer.CurrentState.Value == GameState.Ranking);

        if (rankingPanel.activeSelf != isRanking)
        {
            rankingPanel.SetActive(isRanking);
        }

    }

    void OnBtnClick()
    {
        if (localPlayer != null)
        {
            localPlayer.RequestStandBy();
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
