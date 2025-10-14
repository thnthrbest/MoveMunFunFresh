using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TMPro;
using System.Collections;

public class UDPReceive : MonoBehaviour
{
    Thread receiveThread;
    UdpClient client;
    public int port = 5052;
    public bool startRecieving = true;
    public bool printToConsole = false;
    public string data;
    public TextMeshProUGUI rdanimal_text;
    public TextMeshProUGUI same;
    Color color;
    public string[] a;

    public void Start()
    {
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();

        StartCoroutine(update_ui());
        
        
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
                if (printToConsole) { Debug.Log(data); }
            }
            catch (Exception err)
            {
                //Debug.Log(err.ToString);
            }
        }
    }
    IEnumerator update_ui()
    {
        while(true){
            yield return new WaitForSeconds(0.5f);
            if (!string.IsNullOrEmpty(data)){
                a = (data).Split(' ');
                rdanimal_text.text = a[0];
                if (((float.Parse(a[1])) * 100) >= 70){
                    if (ColorUtility.TryParseHtmlString("#00FF00", out color)) same.color = color;
                }
                else if (((float.Parse(a[1])) * 100) < 70){
                    if (ColorUtility.TryParseHtmlString("#FFD300", out color))same.color = color;
                }
                same.text = ((float.Parse(a[1])) * 100) + " %";

            }else{
                same.text = "0 %";
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
