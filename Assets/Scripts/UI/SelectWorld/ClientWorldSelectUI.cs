using UnityEngine;
using UnityEngine.UI;

public class ClientWorldSelectUI : MonoBehaviour
{
    // UI
    public GameObject panelWorldSelect;

    // 월드 선택 버튼
    public Button btnWorldA;
    public Button btnWorldB;
    public Button btnWorldC;

    private PlayerStateManager localPlayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        btnWorldA.onClick.AddListener(() => OnWorldSeleected(0));
        btnWorldB.onClick.AddListener(() => OnWorldSeleected(1));
        btnWorldC.onClick.AddListener(() => OnWorldSeleected(2));
    }

    // Update is called once per frame
    void Update()
    {
        if(localPlayer == null)
        {
            FindMyPlayer();
            return;
        }

        bool isSelectState = (localPlayer.CurrentState.Value == GameState.SelectWorld);

        if (panelWorldSelect.activeSelf != isSelectState)
        {
            panelWorldSelect.SetActive(isSelectState);
        }
    }

    void OnWorldSeleected(int index)
    {
        if (localPlayer != null)
        {
            Debug.Log($"World {index} selected by player.");
            localPlayer.RequestSelectWorld(index);
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
