using UnityEngine;
using UnityEngine.UI; // จำเป็นสำหรับการควบคุม UI เช่น Image
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    // สร้างรายการ (Array) เพื่อเก็บรูปหัวใจทั้ง 3 ดวง
    public GameObject[] heartImages;

    private int maxHealth;
    private int currentHealth;

    void Start()
    {
        // ตั้งค่าเลือดเริ่มต้นให้เท่ากับจำนวนรูปหัวใจที่เราใส่เข้ามา
        maxHealth = heartImages.Length;
        currentHealth = maxHealth;
    }

    private void OnTriggerEnter(Collider other)
    {
        // ตรวจสอบว่าวัตถุที่ชนด้วยมี Tag "damage" หรือไม่
        if (other.CompareTag("damage"))
        {
            // ถ้าใช่ ให้เรียกฟังก์ชันลดเลือด
            TakeDamage(1);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        // ลดเลือดปัจจุบันลง
        currentHealth -= damageAmount;

        // เรียกฟังก์ชันอัปเดตหน้าจอ
        UpdateHealthUI();

        // (ทางเลือก) ตรวจสอบว่าเลือดหมดหรือยัง
        if (currentHealth <= 0)
        {
            PlayerPrefs.SetString("game_id", "2");
            SceneManager.LoadScene("Endgame");
        }
    }

    void UpdateHealthUI()
    {
        // ตรวจสอบว่าเลือดยังไม่หมด (ป้องกัน Error)
        if (currentHealth >= 0)
        {
            // สั่งปิด GameObject ของรูปหัวใจในตำแหน่งที่ตรงกับเลือดที่เหลือ
            // เช่น เลือดเหลือ 2 (จาก 3) จะปิดรูปหัวใจช่องที่ 2 (ดวงที่ 3)
            heartImages[currentHealth].SetActive(false);
        }
    }
    public void EndRound()
    {
        heartImages[0].SetActive(false);
        heartImages[1].SetActive(false);
        heartImages[2].SetActive(false);
        TakeDamage(3);
    }
}