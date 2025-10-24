using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TMPro;
using System.Collections;

public class uDPReceive : MonoBehaviour
{
    [SerializeField] private uDPSender uDPSender;
    Thread receiveThread;
    UdpClient client;
    public int port = 5052;
    public bool startRecieving = true;
    public bool printToConsole = false;
    public string data;
    public string[] a;
    public void Start()
    {
        StartCoroutine(wait());
    }
    IEnumerator wait()
    {
        yield return new WaitForSeconds(3f);
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
        int sum = int.Parse(uDPSender.num);
        a = new string[sum + 1];
    }
    private void ReceiveData()
    {
        client = new UdpClient(port);
        while (startRecieving)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] dataByte = client.Receive(ref anyIP);
                data = Encoding.UTF8.GetString(dataByte);
                a[int.Parse(data.Split(':')[0])] = data.Split(':')[1];
                if (printToConsole) { Debug.Log(data); }
            }
            catch (Exception err)
            {
                //Debug.Log(err.ToString);
            }
        }
    }

    private void OnDestroy()
    {
        client.Close();
    }
    void OnApplicationQuit()
    {
        client.Close();
    }
}
