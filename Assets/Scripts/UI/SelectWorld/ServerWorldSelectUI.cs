using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ServerWorldSelectUI : MonoBehaviour
{
    // Left UI
    public GameObject panelLeft;
    public TMP_Text textStatusLeft;
    // RIght UI
    public GameObject panelRight;
    public TMP_Text textStatusRight;



    // Update is called once per frame
    void Update()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;

        foreach (var player in FindObjectsByType<PlayerStateManager>(FindObjectsSortMode.None))
        {
            if (player.OwnerClientId == NetworkManager.ServerClientId) continue;

            string role = player.MyRole.Value.ToString();
            bool isSelectState = (player.CurrentState.Value == GameState.SelectWorld);

            if (role == "Left")
            {
                UpdatePanel(panelLeft, textStatusLeft, player, isSelectState);
            }
            else if (role == "Right")
            {
                UpdatePanel(panelRight, textStatusRight, player, isSelectState);
            }
        }
    }

    void UpdatePanel(GameObject panel, TMP_Text text, PlayerStateManager player, bool isActive)
    {
        if (panel.activeSelf != isActive)
        {
            panel.SetActive(isActive);
        }

        if (isActive)
        {
            text.text = $"{player.PlayerName.Value}님이\n월드를 고민 중입니다...";
        }
    }
}
