using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClientNameInputUI : MonoBehaviour
{
    // UI
    public GameObject panelnameInput; // 이름 입력 패널
    public TMP_Text textDisplayName;

    // 버튼
    public Transform keyboardContainer; // 키보드 컨테이너
    public GameObject keyButtonPrefab;
    public Transform[] keyRows; 

    public Button btnBackspace;
    public Button btnConfirm;
    public Button btnShift;

    // 버튼 디자인
    public Color normalColor = Color.white;
    public Color shiftColor = Color.yellow; 
    // 변수
    private PlayerStateManager localPlayer;

    // 한글 처리기
    private HangulAutomata hangul = new HangulAutomata();
    private bool isShifted = false; // Shift 상태

    private List<TMP_Text> keyButtonTexts = new List<TMP_Text>();

    // 일반 상태 키배열 (총 26개)
    private readonly string[] normalKeys = {
        "ㅂ", "ㅈ", "ㄷ", "ㄱ", "ㅅ", "ㅛ", "ㅕ", "ㅑ", "ㅐ", "ㅔ",
        "ㅁ", "ㄴ", "ㅇ", "ㄹ", "ㅎ", "ㅗ", "ㅓ", "ㅏ", "ㅣ",
        "ㅋ", "ㅌ", "ㅊ", "ㅍ", "ㅠ", "ㅜ", "ㅡ"
    };

    // Shift 눌렀을 때 키배열 (쌍자음 + ㅒ, ㅖ)
    private readonly string[] shiftKeys = {
        "ㅃ", "ㅉ", "ㄸ", "ㄲ", "ㅆ", "ㅛ", "ㅕ", "ㅑ", "ㅒ", "ㅖ",
        "ㅁ", "ㄴ", "ㅇ", "ㄹ", "ㅎ", "ㅗ", "ㅓ", "ㅏ", "ㅣ",
        "ㅋ", "ㅌ", "ㅊ", "ㅍ", "ㅠ", "ㅜ", "ㅡ"
    };

    // 각 줄에 들어갈 키의 개수 (10개, 9개, 7개)
    private readonly int[] rowCounts = { 10, 9, 7 };

    void Start()
    {
        CreateKeyboard();

        // 특수 버튼
        btnBackspace.onClick.AddListener(OnBackspaceClicked);
        btnConfirm.onClick.AddListener(OnConfirmClicked);

        // Shift 버튼
        if (btnShift != null)
        {
            btnShift.onClick.AddListener(OnShiftClicked);
            UpdateShiftButtonDesign();
        }
    }

    void CreateKeyboard()
    {
        if (keyRows != null)
        {
            foreach (Transform row in keyRows)
            {
                for (int i = row.childCount - 1; i >= 0; i--)
                {
                    Destroy(row.GetChild(i).gameObject);
                }
            }
        }

        keyButtonTexts.Clear();

        int currentKeyIndex = 0;

        for (int r = 0; r < keyRows.Length; r++)
        {
            // 이 줄에 넣을 개수만큼 반복
            int countInThisRow = rowCounts[r];
            Transform currentRow = keyRows[r];

            for (int k = 0; k < countInThisRow; k++)
            {
                if (currentKeyIndex >= normalKeys.Length) break;

                // 버튼 생성 
                GameObject go = Instantiate(keyButtonPrefab, currentRow);
                TMP_Text btnText = go.GetComponentInChildren<TMP_Text>();

                keyButtonTexts.Add(btnText);
                btnText.text = normalKeys[currentKeyIndex];

                // 이벤트 연결
                int index = currentKeyIndex;
                go.GetComponent<Button>().onClick.AddListener(() => OnKeyClicked(index));

                currentKeyIndex++;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (localPlayer == null)
        {
            FindMyPlayer();
            return;
        }

        // 상태가 EnterName 일때만 패널 활성화
        bool isEnterNameState = (localPlayer.CurrentState.Value == GameState.EnterName);

        if (panelnameInput.activeSelf != isEnterNameState)
        {
            panelnameInput.SetActive(isEnterNameState);

        // 패널 켜질때 이름 초기화
            if (isEnterNameState)
            {
                hangul.Clear();
                isShifted = false;
                RefreshKeyLabels();
                UpdateShiftButtonDesign();
                UpdateDisplay();
            }
        }
    }


    // ---------------------------
    // 버튼 이벤트

    // 키보드 눌렀을 때
    void OnKeyClicked(int index)
    {
        if (textDisplayName.text.Length >= 8) return;

        string charToInput = isShifted ? shiftKeys[index] : normalKeys[index];

        hangul.InputKey(charToInput[0]);

        //if (isShifted) ToggleShift(); 

        UpdateDisplay();
    }

    void OnShiftClicked()
    {
        isShifted = !isShifted;
        RefreshKeyLabels();
        UpdateShiftButtonDesign();
    }


    // --------------------------
    // UI 갱신


    void RefreshKeyLabels()
    {
        for (int i = 0; i < keyButtonTexts.Count; i++)
        {
            if (isShifted)
                keyButtonTexts[i].text = shiftKeys[i];
            else
                keyButtonTexts[i].text = normalKeys[i];
        }
    }


    void UpdateShiftButtonDesign()
    {
        if (btnShift == null) return;

        Image img = btnShift.GetComponent<Image>();
        if (img != null)
        {
            img.color = isShifted ? shiftColor : normalColor;
        }
    }

    // 지우기 버튼
    void OnBackspaceClicked()
    {
        hangul.Backspace();
        UpdateDisplay();
       
    }

    // 확인 버튼 ( 다음 단계로)
    void OnConfirmClicked()
    {
        string finalName = hangul.GetFullText();
        if (finalName.Length > 0 && localPlayer != null)
        {
            localPlayer.RequestNameConfirm(); // 다음 단계 요청
        }
    }

    // 화면 갱신 및 서버 전송
    void UpdateDisplay()
    {
        string currentText = hangul.GetFullText();
        textDisplayName.text = currentText;

        // 실시간 서버 전송
        if (localPlayer != null)
        {
            localPlayer.UpdateName(currentText);
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
