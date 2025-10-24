using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.IO;
using System;
using System.Collections.Generic; 

[System.Serializable]
public class LandmarkData { public int id; public float x; public float y; public float z; public float v; }
[System.Serializable]
public class LandmarkList { public List<LandmarkData> landmarks; }

public class Dec : MonoBehaviour
{
    // ... (ตั้งค่า Network, UI, Prefabs เหมือนเดิม) ...
    [Header("Network Settings")]
    public string serverAddress = "127.0.0.1";
    public int serverPort = 5000;
    [Header("UI & Visualization")]
    public RawImage rawImage;
    public GameObject pointPrefab;
    public GameObject nosePrefab;
    public GameObject bonePrefab; 
    public float boneWidth = 0.2f;
    [Header("Body Scaling")]
    private Vector3 scaleMultiplier = new Vector3(4.5f, 4.5f, 0f); 
    [Header("Starting Point")]
    public Transform startingPointOrigin; 
    [Header("Live Data")]
    public LandmarkList currentLandmarks;
    
    // (ตัวแปรจัดการจุดและเส้น เหมือนเดิม)
    private Dictionary<int, GameObject> points = new Dictionary<int, GameObject>();
    private const int NOSE_ID = 0;
    private class BoneLink { public GameObject boneObject; public int id1; public int id2; }
    private List<BoneLink> bones = new List<BoneLink>();
    private readonly (int, int)[] boneConnections = 
    {
        (11, 12), (12, 24), (24, 23), (23, 11), // Torso
        (11, 13), (13, 15), // Left Arm
        (12, 14), (14, 16), // Right Arm
        (23, 25), (25, 27), (27, 31), // Left Leg
        (24, 26), (26, 28), (28, 32)  // Right Leg
    };
    
    // (ตัวแปรภายในอื่นๆ)
    private WebCamTexture webcamTexture;
    private TcpClient client;
    private NetworkStream stream;
    private BinaryWriter writer;
    private BinaryReader reader;
    private Texture2D frame;
    
    // --- ⭐️ เพิ่มตัวแปรนี้กลับมา ---
    private Texture2D processedTexture; 
    
    void Start()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0) { Debug.LogError("ไม่พบเว็บแคม!"); return; }
        
        webcamTexture = new WebCamTexture(devices[0].name, 640, 480, 30); 
        
        // --- ⭐️ สร้าง Texture นี้ด้วย ---
        processedTexture = new Texture2D(2, 2); 
        
        // ⭐️ (ไม่ต้องตั้งค่า rawImage.texture ที่นี่)
        // rawImage.texture = webcamTexture; 
        webcamTexture.Play();
        
        StartCoroutine(InitializeTextures());
        ConnectToServer();
        currentLandmarks = new LandmarkList();

        if (startingPointOrigin == null) { startingPointOrigin = this.transform; }
    }
    
    System.Collections.IEnumerator InitializeTextures()
    {
        yield return new WaitUntil(() => webcamTexture.width > 16); 
        frame = new Texture2D(webcamTexture.width, webcamTexture.height); 
    }

    void ConnectToServer()
    {
        // ... (เหมือนเดิม)
        try
        {
            client = new TcpClient(serverAddress, serverPort);
            stream = client.GetStream();
            writer = new BinaryWriter(stream);
            reader = new BinaryReader(stream);
        }
        catch (Exception e) { Debug.LogError($"เชื่อมต่อล้มเหลว: {e.Message}"); }
    }

    void Update()
    {
        if (writer == null || reader == null || !webcamTexture.isPlaying || !webcamTexture.didUpdateThisFrame)
        {
            return;
        }

        try
        {
            // 1. ส่งภาพ (เหมือนเดิม)
            frame.SetPixels(webcamTexture.GetPixels());
            frame.Apply();
            byte[] bytes = frame.EncodeToJPG();
            writer.Write(bytes.Length);
            writer.Write(bytes);
            writer.Flush();

            // --- ⭐️ 2. เพิ่มส่วนรับภาพกลับมา ---
            // (นี่คือ Package 1)
            int processedLength = reader.ReadInt32();
            byte[] processedData = reader.ReadBytes(processedLength);
            processedTexture.LoadImage(processedData);
            rawImage.texture = processedTexture; // <-- ⭐️ ตั้งค่า RawImage ที่นี่
            // --- สิ้นสุดส่วนที่เพิ่ม ---

            // 3. รับ JSON (เหมือนเดิม)
            // (นี่คือ Package 2)
            int jsonLength = reader.ReadInt32();
            byte[] jsonData = reader.ReadBytes(jsonLength);
            string jsonString = System.Text.Encoding.UTF8.GetString(jsonData);
            if (jsonString.Length > 0)
            {
                currentLandmarks = JsonUtility.FromJson<LandmarkList>(jsonString);
            }

            // 4. ขยับจุด (เหมือนเดิม)
            UpdatePointVisuals();
            
            // 5. วาดกระดูก (เหมือนเดิม)
            UpdateBoneVisuals();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"การเชื่อมต่อหลุด: {ex.Message}");
            Disconnect(); 
        }
    }
    
    // (ฟังก์ชันที่เหลือ UpdatePointVisuals, UpdateBoneVisuals, Disconnect, 
    // OnApplicationQuit, OnDestroy เหมือนเดิมทั้งหมด)
    
    void UpdatePointVisuals()
    {
        if (currentLandmarks == null || currentLandmarks.landmarks == null || currentLandmarks.landmarks.Count == 0)
            return;
        foreach (LandmarkData lm in currentLandmarks.landmarks)
        {
            if (!points.ContainsKey(lm.id))
            {
                GameObject prefabToUse = (lm.id == NOSE_ID) ? nosePrefab : pointPrefab;
                GameObject newPoint = Instantiate(prefabToUse, startingPointOrigin); 
                points[lm.id] = newPoint;
            }
            Vector3 newPosition = new Vector3(0.5f - lm.x, 0.5f - lm.y, 0f);
            Vector3 scaledPosition = Vector3.Scale(newPosition, scaleMultiplier);
            points[lm.id].transform.localPosition = scaledPosition;
        }
    }
    void UpdateBoneVisuals()
    {
        if (points.Count == 0 || bonePrefab == null) return; 
        if (bones.Count == 0)
        {
            foreach (var (id1, id2) in boneConnections)
            {
                if (points.ContainsKey(id1) && points.ContainsKey(id2))
                {
                    GameObject newBone = Instantiate(bonePrefab, startingPointOrigin); 
                    bones.Add(new BoneLink { boneObject = newBone, id1 = id1, id2 = id2 });
                }
            }
        }
        foreach (var bone in bones)
        {
            GameObject p1_obj = points[bone.id1];
            GameObject p2_obj = points[bone.id2];
            Vector3 p1 = p1_obj.transform.position; 
            Vector3 p2 = p2_obj.transform.position;
            Vector3 midpoint = (p1 + p2) / 2f;
            bone.boneObject.transform.position = midpoint;
            Vector3 direction = p2 - p1;
            float length = direction.magnitude;
            bone.boneObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
            bone.boneObject.transform.localScale = new Vector3(boneWidth, length * 0.5f, boneWidth);
        }
    }
    public void Disconnect()
    {
        if (client == null) return; 
        writer?.Close();
        reader?.Close();
        stream?.Close();
        client?.Close();
        writer = null; reader = null; stream = null; client = null;
    }
    void OnApplicationQuit() { Disconnect(); }
    void OnDestroy()
    {
        Disconnect();
        if (webcamTexture != null) { webcamTexture.Stop(); }
    }
}