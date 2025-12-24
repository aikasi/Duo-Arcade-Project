using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClientNameConfirmUI : MonoBehaviour
{
    // UI
    public GameObject panelConfirm;
    public TMP_Text textFinalName;

    // 버튼 연결
    public Button btnEdit;
    public Button btnStart;

    private PlayerStateManager localPlayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        btnEdit.onClick.AddListener(OnEditClicked);
        btnStart.onClick.AddListener(OnStartClicked);
    }

    // Update is called once per frame
    void Update()
    {
        if (localPlayer == null)
        {
            FindMyPlayer();
            return;
        }

        // 상태가 nameConfirm일 때만 패널 활성화
        bool isConfirmState = (localPlayer.CurrentState.Value == GameState.NameConfirm);

        if (panelConfirm.activeSelf != isConfirmState)
        {
            panelConfirm.SetActive(isConfirmState);

            // 패널 켜질때 이름 보여주기
            if (isConfirmState)
            {
                textFinalName.text = localPlayer.PlayerName.Value.ToString();
            }
        }

    }

    void OnEditClicked()
    {
        if(localPlayer != null)
        {
            localPlayer.RequestNameEdit();
        }
    }

    void OnStartClicked()
    {
        if(localPlayer != null)
        {
            localPlayer.RequestWorldSelect();
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
