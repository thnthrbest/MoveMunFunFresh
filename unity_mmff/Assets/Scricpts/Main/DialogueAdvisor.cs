using UnityEngine;
using TMPro;
using System.Collections;
using System.Linq;

public class DialogueAdvisor : MonoBehaviour
{
    [Header("References")]
    public DashboardController dashboardController;
    public TextMeshProUGUI dialogueadvisor;

    [Header("Game Recommendations")]
    [Tooltip("ชื่อเกมที่แนะนำสำหรับแต่ละส่วน")]
    public string[] recommendedGames = new string[]
    {
        "Jump & Climb Challenge",     // Upper_Body (index 0)
        "Running Adventure",           // Lower_Body (index 1)
        "Speed Dash",                  // Agility (index 2)
        "Stretching Quest",            // Flexibility (index 3)
        "Ball Catch Master"            // Hand-Eye (index 4)
    };

    private int[] point;
    private string[] partNames = new string[]
    {
        "กล้ามเนื้อส่วนบน",
        "กล้ามเนื้อส่วนล่าง",
        "ความว่องไว",
        "ความยืดหยุ่น",
        "ประสานมือกับตา"
    };

    void Start()
    {
        point = new int[5];
        StartCoroutine(WaitAndUpdateAdvice());
    }

    IEnumerator WaitAndUpdateAdvice()
    {
        // รอ 4 วินาทีให้ DashboardController ดึงข้อมูลเสร็จ
        yield return new WaitForSeconds(4f);

        UpdatePointsAndAdvice();
    }

    public void UpdatePointsAndAdvice()
    {
        if (dashboardController == null)
        {
            Debug.LogError("❌ DashboardController reference is missing!");
            dialogueadvisor.text = "ไม่สามารถโหลดข้อมูลได้";
            return;
        }

        for (int i = 0; i < 5; i++)
        {
            point[i] = dashboardController.part[i];
        }

        Debug.Log($"📊 Points loaded: [{string.Join(", ", point)}]");

        // วิเคราะห์และแสดงคำแนะนำ
        GenerateAdvice();
    }

    void GenerateAdvice()
    {
        // หาค่าต่ำสุด
        int minScore = point.Min();

        // หาทุก index ที่มีคะแนนต่ำสุด (กรณีมีหลายส่วนที่คะแนนเท่ากัน)
        var lowestIndices = point
            .Select((score, index) => new { score, index })
            .Where(x => x.score == minScore)
            .Select(x => x.index)
            .ToList();

        string advice = GenerateAdviceText(lowestIndices, minScore);

        // แสดงคำแนะนำ
        if (dialogueadvisor != null)
        {
            dialogueadvisor.text = advice;
        }

        Debug.Log($"💬 Advice generated: {advice}");
    }

    string GenerateAdviceText(System.Collections.Generic.List<int> lowestIndices, int minScore)
    {
        if (lowestIndices.Count == 0)
        {
            return "ไม่พบข้อมูล กรุณาลองใหม่อีกครั้ง";
        }

        // กรณีคะแนนเท่ากันทุกส่วน
        if (lowestIndices.Count == 5)
        {
            return $"🎮 <b>คะแนนทุกส่วนเท่ากันที่ {minScore} คะแนน</b>\n\n" +
                "น้องสามารถเลือกเล่นเกมไหนก็ได้ตามที่ชอบ! 😊\n" +
                "แนะนำให้เริ่มจากเกมที่สนใจหรือสนุกที่สุดเพื่อสร้างแรงจูงใจในการฝึกฝน";
        }

        // กรณีมีหลายส่วนที่คะแนนต่ำสุดเท่ากัน (แต่ไม่ใช่ทั้งหมด)
        if (lowestIndices.Count > 1)
        {
            string parts = string.Join(", ", lowestIndices.Select(i => partNames[i]));
            string games = string.Join(", ", lowestIndices.Select(i => recommendedGames[i]));

            return $"🎯 <b>พบจุดที่ควรพัฒนา!</b>\n\n" +
                $"น้องมีคะแนน<color=#FF6B6B><b>{minScore} คะแนน</b></color>ในส่วน:\n" +
                $"• {parts}\n\n" +
                $"<b>เกมที่แนะนำ:</b>\n{games}\n\n" +
                "💡 เลือกเล่นเกมในส่วนที่อยากพัฒนาก่อนนะ!";
        }

        // กรณีมีเพียงส่วนเดียวที่คะแนนต่ำสุด
        int weakestPart = lowestIndices[0];
        string advice = $"🎯 <b>แนะนำเกมสำหรับน้อง!</b>\n\n";

        // สร้างคำแนะนำเฉพาะแต่ละส่วน
        switch (weakestPart)
        {
            case 0: // Upper_Body
                advice += $"น้องมีคะแนน<color=#FF6B6B><b>{minScore} คะแนน</b></color>ในส่วน<b>{partNames[0]}</b>\n\n" +
                        $"🎮 แนะนำเกม: <b><color=#4CAF50>{recommendedGames[0]}</color></b>\n\n" +
                        "เกมนี้จะช่วยพัฒนา:\n" +
                        "• กล้ามเนื้อแขนและไหล่\n" +
                        "• พลังและความแข็งแรงส่วนบน\n" +
                        "• การทรงตัว";
                break;

            case 1: // Lower_Body
                advice += $"น้องมีคะแนน<color=#FF6B6B><b>{minScore} คะแนน</b></color>ในส่วน<b>{partNames[1]}</b>\n\n" +
                        $"🎮 แนะนำเกม: <b><color=#4CAF50>{recommendedGames[1]}</color></b>\n\n" +
                        "เกมนี้จะช่วยพัฒนา:\n" +
                        "• กล้ามเนื้อขาและสะโพก\n" +
                        "• ความแข็งแรงของขา\n" +
                        "• ความอดทนในการเคลื่อนไหว";
                break;

            case 2: // Agility
                advice += $"น้องมีคะแนน<color=#FF6B6B><b>{minScore} คะแนน</b></color>ในส่วน<b>{partNames[2]}</b>\n\n" +
                        $"🎮 แนะนำเกม: <b><color=#4CAF50>{recommendedGames[2]}</color></b>\n\n" +
                        "เกมนี้จะช่วยพัฒนา:\n" +
                        "• ความเร็วในการเคลื่อนไหว\n" +
                        "• การเปลี่ยนทิศทางอย่างรวดเร็ว\n" +
                        "• ปฏิกิริยาตอบสนอง";
                break;

            case 3: // Flexibility
                advice += $"น้องมีคะแนน<color=#FF6B6B><b>{minScore} คะแนน</b></color>ในส่วน<b>{partNames[3]}</b>\n\n" +
                        $"🎮 แนะนำเกม: <b><color=#4CAF50>{recommendedGames[3]}</color></b>\n\n" +
                        "เกมนี้จะช่วยพัฒนา:\n" +
                        "• ความยืดหยุ่นของกล้ามเนื้อ\n" +
                        "• การเหยียดและยืดตัว\n" +
                        "• ลดการบาดเจ็บ";
                break;

            case 4: // Hand-Eye
                advice += $"น้องมีคะแนน<color=#FF6B6B><b>{minScore} คะแนน</b></color>ในส่วน<b>{partNames[4]}</b>\n\n" +
                        $"🎮 แนะนำเกม: <b><color=#4CAF50>{recommendedGames[4]}</color></b>\n\n" +
                        "เกมนี้จะช่วยพัฒนา:\n" +
                        "• การประสานระหว่างตากับมือ\n" +
                        "• ความแม่นยำในการเคลื่อนไหว\n" +
                        "• สมาธิและจังหวะ";
                break;
        }

        advice += "\n\n<i>💪 มาลองเล่นกันเถอะ!</i>";

        return advice;
    }

    public void OnDateChanged()
    {
        StartCoroutine(WaitAndUpdateAdvice());
    }

    /// <summary>
    /// บังคับอัพเดททันที (สำหรับเรียกจากปุ่มหรือ event อื่นๆ)
    /// </summary>
    public void ForceUpdateAdvice()
    {
        UpdatePointsAndAdvice();
    }
    
}
