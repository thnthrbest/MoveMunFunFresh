using UnityEngine;

public class DestroyOnContact : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("ลาก Prefab ของ 'damage' (เช่น Particle Effect, เสียงระเบิด) มาใส่ที่นี่")]
    public GameObject damagePrefab;

    // --- 1. ⭐️⭐️ แก้ไขตรงนี้ ⭐️⭐️ ---
    [Header("Spawn Target")]
    [Tooltip("ลาก GameObject ที่เป็น 'เป้าหมาย' (mark) สำหรับเสกของมาใส่ที่นี่")]
    public Transform spawnTarget; // <-- เปลี่ยนจาก string เป็น Transform

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("hand") || other.CompareTag("player"))
        {
            ActivateAndDestroy();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("hand") || collision.gameObject.CompareTag("player"))
        {
            ActivateAndDestroy();
        }
    }

    private void ActivateAndDestroy()
    {
        // --- 2. ⭐️⭐️ แก้ไขฟังก์ชันนี้ ⭐️⭐️ ---
        // ตรวจสอบว่าลาก Prefab มาใส่ และ "ลาก spawnTarget มาใส่"
        if (damagePrefab != null && spawnTarget != null)
        {
            // 4. เสก Prefab 'damage' ที่ตำแหน่งและองศาของ "spawnTarget"
            Instantiate(damagePrefab, spawnTarget.position, spawnTarget.rotation);
        }
        else
        {
            Debug.LogWarning("ไม่สามารถเสก 'damage' ได้ เพราะยังไม่ได้ลาก 'spawnTarget' หรือ 'damagePrefab' มาใส่ใน Inspector");
        }
        // --- สิ้นสุดการแก้ไข ---

        // 5. ทำลายตัวเอง (วัตถุที่สคริปต์นี้ติดอยู่)
        Destroy(gameObject);
    }
}