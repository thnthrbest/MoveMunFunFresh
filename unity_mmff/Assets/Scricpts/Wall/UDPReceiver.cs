using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

// ++ ส่วนที่ 1: สร้าง "พิมพ์เขียว" ของข้อมูลที่ได้รับ ++
// ต้องวางไว้นอก Class หลัก
[System.Serializable]
public class LandmarkData
{
    public int id;
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public class LandmarkList
{
    public LandmarkData[] landmarks;
}


public class UDPReceiver : MonoBehaviour
{
    Thread receiveThread;
    UdpClient client;
    public int port = 5052;

    // ++ ส่วนที่ 2: สร้างตัวแปรสาธารณะ (public) ที่ขาดไป ++
    // นี่คือตัวแปรที่ PoseVisualizer ต้องการเรียกใช้
    public LandmarkList receivedData;
    private string lastReceivedPacket = "";
    private object lockObject = new object();

    void Start()
    {
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }
    
    // ++ ส่วนที่ 3: เพิ่ม Update() เพื่อแปลงข้อมูลใน Main Thread ++
    void Update()
    {
        string packetToProcess;
        lock (lockObject)
        {
            packetToProcess = lastReceivedPacket;
        }

        if (!string.IsNullOrEmpty(packetToProcess))
        {
            // แปลง JSON ให้เป็น Object แล้วเก็บไว้ใน receivedData
            receivedData = JsonUtility.FromJson<LandmarkList>("{\"landmarks\":" + packetToProcess + "}");
        }
    }

    private void ReceiveData()
    {
        client = new UdpClient(port);
        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);
                string text = Encoding.UTF8.GetString(data);

                // แค่เก็บข้อมูลล่าสุดที่เป็น Text ไว้ก่อน
                lock(lockObject)
                {
                    lastReceivedPacket = text;
                }
            }
            catch (Exception)
            {
                // จัดการ Error
            }
        }
    }

    void OnApplicationQuit()
    {
        if (receiveThread != null) receiveThread.Abort();
        if (client != null) client.Close();
    }
}