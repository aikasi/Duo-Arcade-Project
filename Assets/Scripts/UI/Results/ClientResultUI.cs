using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ClientResultUI : MonoBehaviour
{
    // UI
    public GameObject resultPanel;
    public Button btnNext;

    private PlayerStateManager localPlayer;

    // 연출시간 지연
    private Coroutine showRoutine;
    private bool isResultVisible = false;

    private const float SERVER_DELAY_TIME = 4.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (btnNext != null)
        {
            btnNext.onClick.AddListener(OnNExtClicked);
        }

        if(resultPanel != null) resultPanel.SetActive(false);
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

        if (isResultState && !isResultVisible)
        {
            if (showRoutine == null)
            {
                showRoutine = StartCoroutine(ShowResultWithDelay());
            }
        }

        else if (!isResultState && isResultVisible)
        {
            HideResultImmediately();
        }
    }

    IEnumerator ShowResultWithDelay()
    {
        yield return new WaitForSeconds(SERVER_DELAY_TIME);

        // 패널 활성화
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }

        isResultVisible = true;
        showRoutine = null;
    }

    void HideResultImmediately()
    {
        if (showRoutine != null)
        {
            StopCoroutine(showRoutine);
            showRoutine = null;
        }

        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
        isResultVisible = false;
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
