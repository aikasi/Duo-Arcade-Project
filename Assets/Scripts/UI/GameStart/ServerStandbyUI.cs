using System.Data;
using Unity.Netcode;
using UnityEngine;

public class ServerStandbyUI : MonoBehaviour
{
    public GameObject leftStandbyPanel;
    public GameObject rightStandbyPanel;


    // Update is called once per frame
    void Update()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;

        // null => 플레이어 없음, true => 활성화, false => 비활성화
        bool? shouldLeftBeActive = null;
        bool? shouldRightBeActive = null;

        // 접속한 클라이언트 수에 따라 StandBy UI 활성화/비활성화
        foreach (var player in FindObjectsByType<PlayerStateManager>(FindObjectsSortMode.None))
        {
            if(player.OwnerClientId == NetworkManager.ServerClientId) continue; // 호스트는 패스

            // 플레이어가 왼쪽 오른쪽 판단
            string role = player.MyRole.Value.ToString();

            if(role == "Left")
            {
                if (shouldLeftBeActive == false) continue; // 이미 비활성화로 결정된 상태면 패스

                shouldLeftBeActive = (player.CurrentState.Value == GameState.StandBy);
            }
            else if(role == "Right")
            {
                if(shouldRightBeActive == false) continue; // 이미 비활성화로 결정된 상태면 패스
                shouldRightBeActive = (player.CurrentState.Value == GameState.StandBy);

            }

            // 상태가 다를때만 실행
            ApplyPanelState(leftStandbyPanel, shouldLeftBeActive);
            ApplyPanelState(rightStandbyPanel, shouldRightBeActive);

        }
    }
    void ApplyPanelState(GameObject panel, bool? shouldBeActive)
        {
            if(panel == null || shouldBeActive == null) return;

            if (panel.activeSelf != shouldBeActive.Value)
            {
                panel.SetActive(shouldBeActive.Value);
            }
        }
  
}
