using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Collections;

public class PythonSocketClient : MonoBehaviour
{
    public string pythonIP = "127.0.0.1";
    public int pythonPort = 5055;

    public Camera sourceCamera;
    public int sendWidth = 640;
    public int sendHeight = 360;
    public RawImage displayImage; // ใช้ RawImage แทน Renderer

    private TcpClient client;
    private NetworkStream stream;
    private Texture2D sendTexture;
    private Texture2D recvTexture;
    private byte[] recvBuffer = new byte[4];
    private Thread recvThread;
    private bool isRunning = true;
    private int MAX_PEOPLE = 3;

    void Start()
    {
        Application.runInBackground = true;
        sendTexture = new Texture2D(sendWidth, sendHeight, TextureFormat.RGB24, false);
        recvTexture = new Texture2D(sendWidth, sendHeight, TextureFormat.RGB24, false);

        try
        {
            client = new TcpClient(pythonIP, pythonPort);
            stream = client.GetStream();
            Debug.Log("[Unity] Connected to Python server");
        }
        catch (Exception e)
        {
            Debug.LogError("[Unity] Connection error: " + e.Message);
        }

        recvThread = new Thread(ReceiveLoop);
        recvThread.Start();

        StartCoroutine(SendLoop());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            MAX_PEOPLE++;
            SendMessageToPython("PEOPLE:" + MAX_PEOPLE);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MAX_PEOPLE = Mathf.Max(1, MAX_PEOPLE - 1);
            SendMessageToPython("PEOPLE:" + MAX_PEOPLE);
        }
    }

    IEnumerator SendLoop()
    {
        while (isRunning)
        {
            yield return new WaitForEndOfFrame();

            if (client == null || !client.Connected) continue;

            RenderTexture rt = new RenderTexture(sendWidth, sendHeight, 24);
            sourceCamera.targetTexture = rt;
            sourceCamera.Render();
            RenderTexture.active = rt;

            sendTexture.ReadPixels(new Rect(0, 0, sendWidth, sendHeight), 0, 0);
            sendTexture.Apply();

            sourceCamera.targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);

            byte[] imageBytes = sendTexture.EncodeToJPG(80);
            byte[] lengthBytes = BitConverter.GetBytes(imageBytes.Length);
            try
            {
                stream.Write(lengthBytes, 0, 4);
                stream.Write(imageBytes, 0, imageBytes.Length);
            }
            catch (Exception e)
            {
                Debug.LogError("[Unity] Send error: " + e.Message);
                break;
            }
        }
    }

    void ReceiveLoop()
    {
        try
        {
            while (isRunning && client != null && client.Connected)
            {
                int read = stream.Read(recvBuffer, 0, 4);
                if (read < 4) break;

                int imgLength = BitConverter.ToInt32(recvBuffer, 0);
                byte[] imgData = new byte[imgLength];
                int totalRead = 0;

                while (totalRead < imgLength)
                {
                    int chunk = stream.Read(imgData, totalRead, imgLength - totalRead);
                    if (chunk == 0) break;
                    totalRead += chunk;
                }

                if (totalRead == imgLength)
                {
                    Texture2D newTex = new Texture2D(2, 2);
                    newTex.LoadImage(imgData);
                    recvTexture = newTex;

                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        if (displayImage != null)
                            displayImage.texture = recvTexture;
                    });
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Unity] Receive loop stopped: " + e.Message);
        }
    }

    public void SendMessageToPython(string msg)
    {
        if (client == null || !client.Connected) return;
        try
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(msg);
            stream.Write(data, 0, data.Length);
        }
        catch (Exception e)
        {
            Debug.LogError("[Unity] SendMessage error: " + e.Message);
        }
    }

    void OnApplicationQuit()
    {
        isRunning = false;
        if (recvThread != null) recvThread.Abort();
        if (stream != null) stream.Close();
        if (client != null) client.Close();
    }
}
