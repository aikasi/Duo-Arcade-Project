using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Unity.Netcode;
using UnityEngine;

public class GameNetworkManager : MonoBehaviour
{
    private Dictionary<ulong, string> clientRoles = new Dictionary<ulong, string>();
    public static GameNetworkManager Instance;

    // UDP 보로드캐스팅
    private UdpClient udpBroadcaster;
    private IPEndPoint broadcastEP;
    private float broadcastInterval = 1.0f; // 1초 간격 발송

    private void Start()
    {
#if UNITY_SERVER || UNITY_EDITOR
        SetupServer();
#endif
    }

    public void SetupServer()
    {
        if (NetworkManager.Singleton.IsServer) return;

        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        // 로그용
        NetworkManager.Singleton.OnClientDisconnectCallback += (id) => Debug.Log($"Client {id} Disconnected.");

        NetworkManager.Singleton.StartHost();
        Debug.Log("호스트 시작");

        // 서버 시작시 발송
        StartBroadcast();
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (request.ClientNetworkId == NetworkManager.ServerClientId)
        {
            Debug.Log("서버 생성");
            response.Approved = true;
            response.CreatePlayerObject = true; // Host도 플레이어 객체는 만듦 (PlayerGunController에서 숨김 처리됨)
            return; 
        }

        // 클라 접속 처리

        string payload = "";
        if (request.Payload != null && request.Payload.Length > 0)
        {
            payload = Encoding.ASCII.GetString(request.Payload);
        }

        bool isRoleTaken = false;
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var state = client.PlayerObject.GetComponent<PlayerStateManager>();
            // 이미 접속한 사람 중, 요청한 역할과 같은 역할이 있는지 확인
            if (state != null && state.MyRole.Value.ToString() == payload)
            {
                isRoleTaken = true;
                break;
            }
        }

        if (isRoleTaken)
        {
            response.Approved = false;
            response.Reason = $"Role '{payload}' is already taken!";
            Debug.LogWarning($"{payload} 역할은 이미 사용 중");
            return;
        }

        if (payload == "Left" || payload == "Right")
        {
            clientRoles[request.ClientNetworkId] = payload;
            response.Approved = true;
            response.CreatePlayerObject = true;
        }
        else
        {
            // 역할이 이상하면 접속 거부 (보안 강화)
            Debug.LogWarning($" 역할: {payload}");
            response.Approved = false;
            response.Reason = "Invalid Role (Must be Left or Right)";
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            // Host는 역할 부여 필요 없음 (패스)
            if (clientId == NetworkManager.ServerClientId) return;

            var playerState = client.PlayerObject.GetComponent<PlayerStateManager>();

            if (playerState != null && clientRoles.ContainsKey(clientId))
            {
                string role = clientRoles[clientId];
                playerState.MyRole.Value = role;

                Debug.Log($" Client {clientId} -> {role}");
                clientRoles.Remove(clientId);
            }
        }
    }


    void StartBroadcast()
    {
        // 포드 47777 사용
        udpBroadcaster = new UdpClient();
        udpBroadcaster.EnableBroadcast = true;
        broadcastEP = new IPEndPoint(IPAddress.Broadcast, 47777);

        StartCoroutine(BroadcastRoutine());
    }

    IEnumerator BroadcastRoutine()
    {
        while (NetworkManager.Singleton.IsServer)
        {
            // 선점 상태 체크
            bool isLeftTaken = false;
            bool isRightTaken = false;

            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                var state = client.PlayerObject.GetComponent<PlayerStateManager>();
                if (state != null)
                {
                    string role = state.MyRole.Value.ToString();
                    if (role == "Left") isLeftTaken = true;
                    if (role == "Right") isRightTaken = true;
                }
            }

            // 메시지 생성 L:1 R:0
            string msg = $"L:{(isLeftTaken ? 1 : 0)}|R:{(isRightTaken ? 1 : 0)}";
            byte[] bytes = Encoding.UTF8.GetBytes(msg);

            // 송출
            try
            {
                udpBroadcaster.Send(bytes, bytes.Length, broadcastEP);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Broadcast Error: {e.Message}");
            }

            yield return new WaitForSeconds(broadcastInterval);
        }
    }

    private void OnDestroy()
    {
        if(udpBroadcaster != null) udpBroadcaster.Close();
    }
}