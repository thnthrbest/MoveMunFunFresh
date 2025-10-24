using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.IO;
using System;
using System.Collections.Generic; 

// (LandmarkData และ LandmarkList เหมือนเดิม)
[System.Serializable]
public class LandmarkData { public int id; public float x; public float y; public float z; public float v; }
[System.Serializable]
public class LandmarkList { public List<LandmarkData> landmarks; }


// --- (คลาสหลัก Dec) ---
public class DecSeen : MonoBehaviour
{
    [Header("Network Settings")]
    public string serverAddress = "127.0.0.1";
    public int serverPort = 5000;

    [Header("UI & Visualization")]
    public RawImage rawImage;
    public GameObject pointPrefab;
    public GameObject nosePrefab;
    public GameObject handPrefab;
    public GameObject bonePrefab; 
    public float boneWidth = 0.05f;

    [Header("Game World Settings")]
    [Tooltip("ลาก BoxCollider ที่กำหนด 'ขอบเขตการขยับ' (Movement Range) มาใส่")]
    public BoxCollider gameAreaBounds; // (A) "ระยะขยับ" (สำหรับแกน X)

    [Tooltip("ขนาดของ 'ตัวละคร' (Skeleton Size) (เช่น 5, 5, 0)")]
    public Vector3 bodyScale = new Vector3(5f, 5f, 0f); // (B) "ขนาดตัว"

    [Header("Starting Point")]
    [Tooltip("จุด Parent ของข้อต่อทั้งหมด (จะใช้เป็น 'ศูนย์กลาง' ที่ไม่ขยับ)")]
    public Transform startingPointOrigin; // "ศูนย์กลาง"
    
    // (ตัวแปรภายใน)
    private Vector3 actualGameSize; 

    [Header("Live Data")]
    public LandmarkList currentLandmarks;
    
    // (ตัวแปรจัดการจุดและเส้น)
    private Dictionary<int, GameObject> points = new Dictionary<int, GameObject>();
    private const int NOSE_ID = 0;
    private const int LEFT_HAND_ID = 15;
    private const int RIGHT_HAND_ID = 16;
    private const int LEFT_HIP_ID = 23;
    private const int RIGHT_HIP_ID = 24;
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
    
    // (ตัวแปร Network และ Webcam)
    private WebCamTexture webcamTexture;
    private TcpClient client;
    private NetworkStream stream;
    private BinaryWriter writer;
    private BinaryReader reader;
    private Texture2D frame;
    private Texture2D processedTexture;
    private bool isReady = false;
    
    
    void Start()
    {
        // (1. ตรวจสอบ BoxCollider)
        if (gameAreaBounds == null)
        {
            Debug.LogError("!!! [Dec.cs] Error: 'Game Area Bounds' (BoxCollider) ยังไม่ได้ลากมาใส่! ปิดการทำงานสคริปต์.");
            this.enabled = false; 
            return;
        }
        actualGameSize = gameAreaBounds.size; // (อ่านแค่ Size)
        Debug.Log("[Dec.cs] อ่านค่า 'Size' (ระยะขยับ) จาก " + gameAreaBounds.name + " สำเร็จ, ขนาดคือ: " + actualGameSize);


        // (2. ตรวจสอบ Starting Point)
        if (startingPointOrigin == null) {
            Debug.LogWarning("[Dec.cs] 'Starting Point Origin' ไม่ได้ตั้งค่า, จะใช้ GameObject นี้เป็น Center แทน");
            startingPointOrigin = this.transform; 
        }

        // (3. ส่วนที่เหลือของ Start() เหมือนเดิม)
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0) { Debug.LogError("ไม่พบเว็บแคม!"); this.enabled = false; return; }
        
        webcamTexture = new WebCamTexture(devices[0].name, 640, 480, 30); 
        processedTexture = new Texture2D(2, 2); 
        webcamTexture.Play();
        
        StartCoroutine(InitializeTextures());
        ConnectToServer();
        currentLandmarks = new LandmarkList();
    }
    
    System.Collections.IEnumerator InitializeTextures()
    {
        yield return new WaitUntil(() => webcamTexture.width >= 640); 
        frame = new Texture2D(webcamTexture.width, webcamTexture.height); 
        isReady = true; 
        Debug.Log("Webcam พร้อมแล้ว!");
    }

    void ConnectToServer()
    {
        try {
            client = new TcpClient(serverAddress, serverPort);
            stream = client.GetStream();
            writer = new BinaryWriter(stream);
            reader = new BinaryReader(stream);
        } catch (Exception e) { Debug.LogError($"เชื่อมต่อล้มเหลว: {e.Message}"); }
    }
    
    void Update()
    {
        if (!isReady || writer == null || reader == null || !webcamTexture.isPlaying || !webcamTexture.didUpdateThisFrame) {
            return;
        }
        try {
            // (รับ/ส่งข้อมูล - ย่อไว้)
            frame.SetPixels(webcamTexture.GetPixels()); frame.Apply();
            byte[] bytes = frame.EncodeToJPG();
            writer.Write(bytes.Length); writer.Write(bytes); writer.Flush();
            int pLen = reader.ReadInt32(); byte[] pData = reader.ReadBytes(pLen);
            processedTexture.LoadImage(pData); rawImage.texture = processedTexture;
            int jLen = reader.ReadInt32(); byte[] jData = reader.ReadBytes(jLen);
            string jsonString = System.Text.Encoding.UTF8.GetString(jData);
            if (jsonString.Length > 0) { currentLandmarks = JsonUtility.FromJson<LandmarkList>(jsonString); }

            UpdatePointVisuals(); 
            UpdateBoneVisuals();
            
        } catch (Exception ex) {
            Debug.LogWarning($"การเชื่อมต่อหลุด: {ex.Message}");
            Disconnect(); 
        }
    }

    void UpdatePointVisuals()
    {
        if (currentLandmarks == null || currentLandmarks.landmarks == null || currentLandmarks.landmarks.Count == 0)
            return;

        // --- 1. หา "ศูนย์กลางสะโพก" (Hip Center) ---
        LandmarkData leftHip = currentLandmarks.landmarks.Find(lm => lm.id == LEFT_HIP_ID);
        LandmarkData rightHip = currentLandmarks.landmarks.Find(lm => lm.id == RIGHT_HIP_ID);

        Vector3 hipCenter = new Vector3(0.5f, 0.5f, 0f); 
        bool hipsFound = false;

        if (leftHip != null && rightHip != null) {
            hipCenter = new Vector3((leftHip.x + rightHip.x) / 2f, (leftHip.y + rightHip.y) / 2f, 0f);
            hipsFound = true;
        }

        // --- 2. (Component A) คำนวณ "ตำแหน่งของตัวละคร" (Movement Offset) ---
        Vector3 hipRawPos = new Vector3(
            0.5f - hipCenter.x,  // (คำนวณเฉพาะ X)
            0f, 
            0f
        ); 
        Vector3 skeletonMovementOffset = Vector3.Scale(hipRawPos, actualGameSize);

        // --- 3. ขยับ "ข้อต่อ" (Joints) ภายในตัวละคร ---
        foreach (LandmarkData lm in currentLandmarks.landmarks)
        {
            if (!points.ContainsKey(lm.id)) {
                GameObject prefabToUse;
                if (lm.id == NOSE_ID) { prefabToUse = nosePrefab; }
                else if (lm.id == LEFT_HAND_ID || lm.id == RIGHT_HAND_ID) {
                    prefabToUse = (handPrefab != null) ? handPrefab : pointPrefab; 
                }
                else { prefabToUse = pointPrefab; }
                
                GameObject newPoint = Instantiate(prefabToUse, startingPointOrigin); 
                points[lm.id] = newPoint;
            }

            // --- 4. (Component B) คำนวณ "ตำแหน่งข้อต่อ" (Animation) ---
            Vector3 relativePos = new Vector3(
                hipCenter.x - lm.x, 
                hipCenter.y - lm.y, 
                0f
            );
            if (!hipsFound) {
                 relativePos = new Vector3(0.5f - lm.x, 0.5f - lm.y, 0f);
            }
            Vector3 scaledBodyPos = Vector3.Scale(relativePos, bodyScale); 

            // --- 5. (A + B) รวมการขยับและการเคลื่อนไหว ---
            Vector3 finalLocalPos = new Vector3(
                skeletonMovementOffset.x + scaledBodyPos.x, // X = (Movement) + (Animation)
                scaledBodyPos.y,                           // Y = (Animation only)
                0f
            );
            
            points[lm.id].transform.localPosition = finalLocalPos;
        }
    }

    // --- (ฟังก์ชันที่เหลือ - ต้องมีแค่ 1 ชุด) ---
    void UpdateBoneVisuals()
    {
        if (points.Count == 0 || bonePrefab == null) return; 
        if (bones.Count == 0) {
            foreach (var (id1, id2) in boneConnections) {
                if (points.ContainsKey(id1) && points.ContainsKey(id2)) {
                    GameObject newBone = Instantiate(bonePrefab, startingPointOrigin); 
                    bones.Add(new BoneLink { boneObject = newBone, id1 = id1, id2 = id2 });
                }
            }
        }
        foreach (var bone in bones) {
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
    
    void OnApplicationQuit() 
    { 
        Disconnect(); 
    }
    
    void OnDestroy()
    {
        Disconnect();
        if (webcamTexture != null) { webcamTexture.Stop(); }
    }
}