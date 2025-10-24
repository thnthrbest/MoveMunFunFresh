using UnityEngine;
using System.Collections.Generic;

public class SpawnOnDoubleTouch : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("ลาก Prefab 'point' ที่คุณต้องการเสกมาใส่ที่นี่")]
    public GameObject pointPrefab;

    // --- 1. ⭐️⭐️ แก้ไขตรงนี้ ⭐️⭐️ ---
    [Header("Spawn Target")]
    [Tooltip("ลาก GameObject ที่เป็น 'เป้าหมาย' (mark) สำหรับเสกของมาใส่ที่นี่")]
    public Transform spawnTarget; // <-- เปลี่ยนจาก string เป็น Transform

    // --- (เราไม่ต้องใช้ markObjectName หรือ markTransform แล้ว) ---

    // ตัวแปรสำหรับนับมือที่กำลังสัมผัส
    private List<Collider> handsTouching = new List<Collider>();

    // "ธง" ป้องกันการทำงานซ้ำ
    private bool actionDone = false;

    void Start()
    {
        // --- 2. ⭐️⭐️ แก้ไขการตรวจสอบ ⭐️⭐️ ---
        // (ลบการค้นหา GameObject.Find ทิ้งไป)

        // ตรวจสอบว่าลาก Prefab มาใส่หรือยัง
        if (pointPrefab == null)
        {
            Debug.LogError("ยังไม่ได้ลาก 'pointPrefab' มาใส่ใน Inspector ของ " + gameObject.name);
        }
        
        // ตรวจสอบว่าลากเป้าหมายมาใส่หรือยัง
        if (spawnTarget == null)
        {
            Debug.LogError("ยังไม่ได้ลาก 'spawnTarget' (เป้าหมาย) มาใส่ใน Inspector ของ " + gameObject.name);
        }
    }

    void Update()
    {
        // 3. ตรวจสอบว่ามีมือ 2 ข้างแตะ และยังไม่ได้ทำงาน (actionDone == false)
        if (handsTouching.Count >= 2 && !actionDone)
        {
            // 4. ตั้งธงทันที ป้องกันการทำงานซ้ำ
            actionDone = true;

            // 5. เรียกฟังก์ชันทำงาน
            SpawnAtTargetAndDestroy();
        }
    }

    private void SpawnAtTargetAndDestroy()
    {
        // --- 6. ⭐️⭐️ แก้ไขฟังก์ชันนี้ ⭐️⭐️ ---
        // ตรวจสอบว่าทุกอย่างพร้อม (ลาก Prefab และ spawnTarget มาแล้ว)
        if (spawnTarget != null && pointPrefab != null)
        {
            // 7. เสก "point" ที่ตำแหน่งและองศาของ "spawnTarget"
            Instantiate(pointPrefab, spawnTarget.position, spawnTarget.rotation);
            Debug.Log("เสก 'point' ที่ตำแหน่งของ " + spawnTarget.name + " สำเร็จ!");
        }
        else
        {
            Debug.LogWarning("ไม่สามารถเสก 'point' ได้ เพราะยังไม่ได้ตั้งค่า Prefab หรือ Spawn Target");
        }

        // 8. ทำลายตัวเอง (วัตถุที่สคริปต์นี้ติดอยู่)
        Destroy(gameObject);
    }

    // (ฟังก์ชัน OnTriggerEnter และ OnTriggerExit เหมือนเดิมครับ)

    // 9. ตรวจจับเมื่อ "มือ" เข้ามา
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("hand") && !actionDone)
        {
            if (!handsTouching.Contains(other))
            {
                handsTouching.Add(other);
                Debug.Log("มือแตะ: " + other.name + " | จำนวน: " + handsTouching.Count);
            }
        }
    }

    // 10. ตรวจจับเมื่อ "มือ" ออกไป
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("hand") && !actionDone)
        {
            if (handsTouching.Contains(other))
            {
                handsTouching.Remove(other);
                Debug.Log("มือออก: " + other.name + " | จำนวน: " + handsTouching.Count);
            }
        }
    }
}