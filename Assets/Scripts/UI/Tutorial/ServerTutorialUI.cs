using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ServerTutorialUI : MonoBehaviour
{
    // UI
    public GameObject panelLeftTutoral;
    public GameObject panelRightTutoral;

    // Image
    public Image imgLeftDisplay;
    public Image imgRightDisplay;

    // Tutoral Image as worldId
    public Sprite[] tutorialSprites; // 0: Space, 1: Ocean, 2: HumanBody


    // Update is called once per frame
    void Update()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        foreach (var player in FindObjectsByType<PlayerStateManager>(FindObjectsSortMode.None))
        {
            if (player.OwnerClientId == NetworkManager.ServerClientId) continue;

            string role = player.MyRole.Value.ToString();

            bool isTutorialState = (player.CurrentState.Value == GameState.Tutorial);

            if (role == "Left")
            {
               UpdatePanel(panelLeftTutoral, imgLeftDisplay, isTutorialState, player.SelectedWorldId.Value);
            }
            else if (role == "Right")
            {
                UpdatePanel(panelRightTutoral, imgRightDisplay, isTutorialState, player.SelectedWorldId.Value);
            }
        }
        
    }

    void UpdatePanel(GameObject panel, Image displayImage, bool isActive, int worldId)
    {
        if (panel.activeSelf != isActive)
        {
            panel.SetActive(isActive);
        }

        if (isActive && displayImage != null && tutorialSprites != null)
        {
            if (worldId >= 0 && worldId < tutorialSprites.Length)
            {
                if (displayImage.sprite != tutorialSprites[worldId])
                {
                    displayImage.sprite = tutorialSprites[worldId];
                }
            }
        }

    }
}
