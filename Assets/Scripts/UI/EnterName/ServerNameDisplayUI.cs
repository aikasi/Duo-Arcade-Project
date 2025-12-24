using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ServerNameDisplayUI : MonoBehaviour
{
    // UI
    public GameObject panelLeft;
    public TMP_Text textNameLeft;

    public GameObject panelRight;
    public TMP_Text textNameRight;


    // Update is called once per frame
    void Update()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;

        foreach (var player in FindObjectsByType<PlayerStateManager>(FindObjectsSortMode.None))
        {
            // 호스트 무시
            if (player.OwnerClientId == NetworkManager.ServerClientId) continue;
            
            string role = player.MyRole.Value.ToString();
            bool isEnterName = (player.CurrentState.Value == GameState.EnterName);

            if (role == "Left")
            {
                UpdatePanel(panelLeft, textNameLeft, player, isEnterName);
            }
            else if (role == "Right")
            {
                UpdatePanel(panelRight, textNameRight, player, isEnterName);
            }
            
        }
    }

    void UpdatePanel(GameObject panel, TMP_Text textName, PlayerStateManager player, bool isActive)
    {
        if (panel.activeSelf != isActive)
        {
            panel.SetActive(isActive);
        }
    
        if (isActive)
        {
            // 실시간 계속 이름 업데이트
            textName.text = player.PlayerName.Value.ToString();
        }

    }
}
