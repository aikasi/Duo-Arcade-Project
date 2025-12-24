using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class ClientTitleUI : MonoBehaviour
{
    public Button btnLeft;
    public Button BtnRight;
    public GameObject uiPanel;
    public TMP_InputField inputIpAddress;
    // 파일 저장 경로
    private string configPath;

    // 재접속용 번수
    private string lastRole = "";
    private string lastIp = "127.0.0.1";
    private bool isReconnecting = false;

    // UDP 수신용
    private UdpClient udpListener;
    private bool isListening = false;
    private const int BROADCAST_PORT = 47777; // 서버와 동일해야함


    private void Start()
    {
        // 실행 파일 옆에 설정
        configPath = Application.dataPath + "/../config.json";

        // 연결 끊김 이벤트 구독
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        // 실행 파일이 있으면 설정 불러오기
        if (File.Exists(configPath))
        {
            string savedRole = File.ReadAllText(configPath).Trim();
        
            if(savedRole == "Left" || savedRole == "Right")
            {
                Connect(savedRole);
                return;
            }
        }
       
        // 파일 없다면 선택
        uiPanel.SetActive(true);

        btnLeft.onClick.AddListener(() => OnRoleSelected("Left"));
        BtnRight.onClick.AddListener(() => OnRoleSelected("Right"));

        StartListening();
    }

    private void Update()
    {
        // UDP 패킷 수신 , UI 갱신
        if (isListening && udpListener != null && udpListener.Available > 0)
        {
            try
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                byte[] bytes = udpListener.Receive(ref remoteEP);
                string msg = Encoding.UTF8.GetString(bytes);

                // 메시지, 버튼 상태 업데이트
                UpdateRoleButtons(msg);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"UDP Receive Error: {e.Message}");
            }
        }
    }

    // -----------------------------
    // UDP 로직

    //서버 메시지("L:1|R:0")를 해석
    private void UpdateRoleButtons(string msg)
    {
        bool leftTaken = msg.Contains("L:1");
        bool rightTaken = msg.Contains("R:1");

        if (btnLeft != null)
        {
            btnLeft.interactable = !leftTaken;
        }

        if (BtnRight != null)
        {
            BtnRight.interactable = !rightTaken;
        }
    }

    // --- UDP 리스닝 관련 함수 ---
    void StartListening()
    {
        if (isListening) return;

        try
        {
            udpListener = new UdpClient(BROADCAST_PORT);
            udpListener.EnableBroadcast = true;
            isListening = true;
            Debug.Log($"UDP 리스너 시작 (Port {BROADCAST_PORT})");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"UDP Start Failed: {e.Message}");
        }
    }

    void StopListening()
    {
        if (udpListener != null)
        {
            udpListener.Close();
            udpListener = null;
        }
        isListening = false;
    }
    private void OnDestroy()
    {
        StopListening();

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    //----------------------------
    // 접속 로직

    void OnRoleSelected(string role)
    {
     File.WriteAllText(configPath, role);
      Debug.Log("Role saved to config: " + role);
        Connect(role);
    }

    void Connect(string role)
    {
        lastRole = role;
        Debug.Log("Connecting as: " + role);

        uiPanel.SetActive(false);

        // 역할(Role) 데이터 설정
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(role);

        // 서버 IP 주소 설정 로직
        string targetIp = "127.0.0.1"; // 기본값

        // UI에 입력된 값이 있다면 그 IP 사용
        if (inputIpAddress != null && !string.IsNullOrEmpty(inputIpAddress.text))
        {
            targetIp = inputIpAddress.text;
        }

        lastIp = targetIp;

        // UnityTransport 컴포넌트 IP 설정
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport != null)
        {
            transport.SetConnectionData(targetIp, 7777);
        }

        NetworkManager.Singleton.StartClient();

    }


    private void OnClientDisconnected(ulong clientId)
    {
        // 클라이언트 입장-> 서버 끊김
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            Debug.Log("서버 끊김.. 재접속 시도");

            // 초기화
            if(uiPanel != null) uiPanel.SetActive(true);

            // 재접속 루틴
            if (!isReconnecting)
            {
                StartCoroutine(AutoReconnectRoutine());
            }
        }
    }

    IEnumerator AutoReconnectRoutine()
    {
        isReconnecting = true;

        while (!NetworkManager.Singleton.IsClient)
        {
            Debug.Log("1초 간격 재접속 시도중");

            // 저장정보로 재설정
            NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(lastRole);
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport> ();
            if (transport != null) transport.SetConnectionData(lastIp, 7777);

            NetworkManager.Singleton.StartClient();

            yield return new WaitForSeconds(1f);
        }

        Debug.Log("재접속 성공!");
        isReconnecting = false;

        if(uiPanel != null) uiPanel.SetActive(false);
    }


}

