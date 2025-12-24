
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class ServerAutoStart : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport != null)
        {
            transport.SetConnectionData("0.0.0.0", 7777);
        }

        NetworkManager.Singleton.StartHost();
        Debug.Log("Server started automatically (Binding to 0.0.0.0).");
    }

}
