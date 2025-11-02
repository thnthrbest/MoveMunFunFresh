using UnityEngine;
using TMPro; // ต้องมีบรรทัดนี้เพื่อควบคุม TextMeshPro

public class PlayerScoreController : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    private static int score = 0;

    void Start()
    {
        // รีเซ็ตคะแนนและอัปเดต UI เมื่อเกมเริ่ม
        score = 0;
        UpdateScoreText();
    }

    private void OnTriggerEnter(Collider other)
    {
        // ตรวจสอบว่าสิ่งที่ชนด้วยมี Tag ชื่อ "point" หรือไม่
        if (other.CompareTag("point"))
        {
            // เพิ่มคะแนน
            AddScore(10);
            
            // ทำลายวัตถุ point ที่ชนด้วย
            Destroy(other.gameObject);
        }
    }

    void AddScore(int pointsToAdd)
    {
        score += pointsToAdd;
        PlayerPrefs.SetInt("score", score);
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "" + score;
        }
    }
}