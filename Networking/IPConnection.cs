using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using UnityEngine;

public class IPConnection
{
    NetworkManager sdp_NetworkManager;
    public string IP = "192.168.0.101";
    public int Port = 53;

    private bool IPAndPortChanged = false;

    public IPConnection(ref NetworkManager incoming_NetworkManager)
    {
        sdp_NetworkManager = incoming_NetworkManager;
        SetConnectionData();
    }

    private void SetConnectionData()
    {
        var utp = (UnityTransport)sdp_NetworkManager.NetworkConfig.NetworkTransport;
        utp.SetConnectionData(IP, (ushort)this.Port);
    }

    public void SetIPAndPort(string IP, int Port)
    {
        this.IP = IP;
        this.Port = Port;

        Debug.Log(IP + " " + Port);

        SetConnectionData();

        IPAndPortChanged = true;
    }

    public void StartHost()
    {
        if(! IPAndPortChanged)
        {
            Debug.LogWarning("IP and Port not configured, using default IP and Port");
        }

        sdp_NetworkManager.StartHost();
    }

    public void StartClient()
    {
        if (!IPAndPortChanged)
        {
            Debug.LogWarning("IP and Port not configured, using default IP and Port");
        }

        sdp_NetworkManager.StartClient();
    }
}
