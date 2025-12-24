using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ServerNameConfirmUI : MonoBehaviour
{
    // Left
    public GameObject panelLeft;
    public TMP_Text textNameLeft;

    // RIght
    public GameObject panelRight;
    public TMP_Text textNameRight;


    // Update is called once per frame
    void Update()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;

        foreach (var player in FindObjectsByType<PlayerStateManager>(FindObjectsSortMode.None))
        {
            if (player.OwnerClientId == NetworkManager.ServerClientId) continue;

            string role = player.MyRole.Value.ToString();

            // 상태 확인
            bool isConfirmState = (player.CurrentState.Value == GameState.NameConfirm);

            if(role == "Left")
            {
                UpdatePanel(panelLeft, textNameLeft, player, isConfirmState);
            }
            else if(role == "Right")
            {
                UpdatePanel(panelRight, textNameRight, player, isConfirmState);
            }
        }
    }

    void UpdatePanel(GameObject panel, TMP_Text text, PlayerStateManager player,bool isActive)
    {
        if (panel.activeSelf != isActive)
        {
            panel.SetActive(isActive);
        }

        if(isActive)
        {
            text.text = $"{player.PlayerName.Value}";
        }
    }
    
}
