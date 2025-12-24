using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ServerRankingUI : MonoBehaviour
{
    [System.Serializable]
    public class RankingPanel
    {
        public GameObject panelObj;

        // 1~7등 슬롯
        public TMP_Text[] nameTexts;
        public TMP_Text[] scoreTexts;
    }

    public RankingPanel leftRanking;
    public RankingPanel rightRanking;

    void Update()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;

        foreach (var player in FindObjectsByType<PlayerStateManager>(FindObjectsSortMode.None))
        {
            if (player.OwnerClientId == NetworkManager.ServerClientId) continue;

            string role = player.MyRole.Value.ToString();

            // 랭킹 상태?
            bool isRankingState = (player.CurrentState.Value == GameState.Ranking);

            if (role == "Left")
            {
                UpdateRankingPanel(leftRanking, isRankingState);
            }
            else if (role == "Right")
            {
                UpdateRankingPanel(rightRanking, isRankingState);
            }
        }
    }


    void UpdateRankingPanel(RankingPanel ui, bool isActive)
    {
        if (ui.panelObj.activeSelf != isActive)
        {
            ui.panelObj.SetActive(isActive);

            // 켜질 때만 데이터 갱신
            if (isActive) RefreshRankingData(ui);
        }
    }

    void RefreshRankingData(RankingPanel ui)
    {
        if(RankingManager.Instance == null) return;

        // 굥유 데이터 랭킹 가져오기 -> 사용
        List<RankingManager.RankData> data = RankingManager.Instance.GetRankings();

        // 텍스트 슬롯
        for (int i = 0; i < ui.nameTexts.Length; i++)
        {
            // 데이터 여부에 따라 표시
            if (i < data.Count)
            {
                if (ui.nameTexts[i]) ui.nameTexts[i].text = data[i].name;
                if (ui.scoreTexts[i]) ui.scoreTexts[i].text = data[i].score.ToString();
            }
            else
            {
                if (ui.nameTexts[i]) ui.nameTexts[i].text = "";
                if (ui.scoreTexts[i]) ui.scoreTexts[i].text = "";
            }
        }
    }

}
