using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("ลาก Prefab ทั้งหมดที่คุณต้องการสุ่มเสกมาใส่ที่นี่")]
    public GameObject[] objectToSpawnPrefabs; 

    [Tooltip("Parent (แม่) ของวัตถุที่เสกออกมา")]
    public Transform spawnParent;

    [Tooltip("ให้เริ่มสุ่มเสกอัตโนมัติเมื่อเริ่มเกมหรือไม่")]
    public bool spawnOnStart = true;
    
    [Tooltip("ช่วงเวลาในการสุ่มเสก (วินาที)")]
    public float spawnInterval = 2f;

    [Tooltip("ปุ่มสำหรับทดสอบการสุ่มเสก")]
    public KeyCode manualSpawnKey = KeyCode.Space;

    // --- 1. ⭐️⭐️ เพิ่มช่องนี้เข้ามา ⭐️⭐️ ---
    [Header("Spawn Target Reference")]
    [Tooltip("ลากวัตถุเป้าหมาย (เช่น 'mark') ที่คุณต้องการส่งค่าตำแหน่งไปให้ Prefab")]
    public Transform targetToPass; // นี่คือ "mark" หรือเป้าหมายของคุณ

    // (ตัวแปรภายในเหมือนเดิม)
    private BoxCollider spawnAreaBox;
    private float timer = 0f;

    void Start()
    {
        // (ส่วน Start() เหมือนเดิมครับ)
        spawnAreaBox = GetComponent<BoxCollider>();
        if (spawnAreaBox == null)
        {
            Debug.LogError("ObjectSpawner: ไม่พบ BoxCollider!");
            this.enabled = false; 
        }
        if (objectToSpawnPrefabs == null || objectToSpawnPrefabs.Length == 0)
        {
             Debug.LogError("ObjectSpawner: ยังไม่ได้ใส่ Prefab!");
             this.enabled = false;
        }
        if (spawnOnStart)
        {
            SpawnObject();
        }
    }

    void Update()
    {
        // (ส่วน Update() เหมือนเดิมครับ)
        if (spawnOnStart)
        {
            timer += Time.deltaTime;
            if (timer >= spawnInterval)
            {
                SpawnObject();
                timer = 0f; 
            }
        }
        if (Input.GetKeyDown(manualSpawnKey))
        {
            SpawnObject();
        }
    }

    public void SpawnObject()
    {
        if (objectToSpawnPrefabs == null || objectToSpawnPrefabs.Length == 0 || spawnAreaBox == null)
        {
            return; 
        }

        // (ส่วนสุ่มตำแหน่ง และ สุ่ม Prefab เหมือนเดิม)
        Vector3 center = spawnAreaBox.center;
        Vector3 size = spawnAreaBox.size;
        float randomX = Random.Range(center.x - size.x / 2f, center.x + size.x / 2f);
        float randomY = Random.Range(center.y - size.y / 2f, center.y + size.y / 2f);
        float randomZ = Random.Range(center.z - size.z / 2f, center.z + size.z / 2f);
        Vector3 randomLocalPosition = new Vector3(randomX, randomY, randomZ);
        Vector3 randomWorldPosition = transform.TransformPoint(randomLocalPosition);
        int randomIndex = Random.Range(0, objectToSpawnPrefabs.Length);
        GameObject prefabToUse = objectToSpawnPrefabs[randomIndex];

        // 6. สุ่มเสกวัตถุ (Instantiate)
        GameObject newObject = Instantiate(prefabToUse, randomWorldPosition, Quaternion.identity);

        if (spawnParent != null)
        {
            newObject.transform.SetParent(spawnParent);
        }

        // --- 2. ⭐️⭐️ นี่คือส่วนที่เพิ่มเข้ามา ⭐️⭐️ ---
        // (หลังจากเสกแล้ว ให้ส่งค่า "เป้าหมาย" (mark) เข้าไป)
        
        if (targetToPass != null)
        {
            // พยายามหาสคริปต์ DestroyOnContact บน Prefab ที่เพิ่งเสก
            DestroyOnContact doc = newObject.GetComponent<DestroyOnContact>();
            if (doc != null)
            {
                // ถ้าเจอ ให้ตั้งค่า spawnTarget ของมัน
                doc.spawnTarget = targetToPass;
                Debug.Log("ส่งค่า Target ให้ " + newObject.name + " (DestroyOnContact)");
            }

            // พยายามหาสคริปต์ SpawnOnDoubleTouch บน Prefab ที่เพิ่งเสก
            SpawnOnDoubleTouch sdt = newObject.GetComponent<SpawnOnDoubleTouch>();
            if (sdt != null)
            {
                // ถ้าเจอ ให้ตั้งค่า spawnTarget ของมัน
                sdt.spawnTarget = targetToPass;
                Debug.Log("ส่งค่า Target ให้ " + newObject.name + " (SpawnOnDoubleTouch)");
            }
        }
        // --- สิ้นสุดส่วนที่เพิ่ม ---

        Debug.Log("สุ่มเสก " + newObject.name + " ที่ตำแหน่ง " + randomWorldPosition);
    }

    // (ฟังก์ชัน OnDrawGizmos() เหมือนเดิมครับ)
    private void OnDrawGizmos()
    {
        if (spawnAreaBox == null)
        {
            spawnAreaBox = GetComponent<BoxCollider>();
            if (spawnAreaBox == null) return;
        }
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(0, 1, 0, 0.3f); 
        Gizmos.DrawCube(spawnAreaBox.center, spawnAreaBox.size);
    }
}