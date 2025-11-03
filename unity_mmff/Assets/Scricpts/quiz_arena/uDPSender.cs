using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Text;

public class uDPSender : MonoBehaviour
{
    UdpClient udpClient;
    public string ip = "127.0.0.1";
    public int port = 6051;
    public string num;

    void Awake()
    {
        udpClient = new UdpClient();
    }

    public void Start()
    {
        num = (PlayerPrefs.GetInt("CountChild")).ToString();
    }

    // Update is called once per frame
    public void Sender(string num)
    {
            byte[] data = Encoding.UTF8.GetBytes(num);
            udpClient.Send(data, data.Length, ip, port);
            Debug.Log("Sent: " + num);
    }
    
    void Exits()
    {
        byte[] data = Encoding.UTF8.GetBytes("stop");
        udpClient.Send(data, data.Length, ip, port);
        Debug.Log("Quit");
    }
    void OnApplicationQuit()
    {
        Exits();
    }

    void OnDestroy()
    {
        Exits();

    }
}
