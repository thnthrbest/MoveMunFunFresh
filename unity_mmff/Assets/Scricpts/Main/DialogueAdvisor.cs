using UnityEngine;
using TMPro;
using System.Collections;
using System.Linq;

public class DialogueAdvisor : MonoBehaviour
{
    [Header("References")]
    public DashboardController dashboardController;
    public TextMeshProUGUI dialogueadvisor;

    [Header("Animation Settings")]
    [Tooltip("ความเร็วในการพิมพ์ (วินาทีต่อตัวอักษร)")]
    public float typingSpeed = 0.015f; // ลดลงเหลือครึ่งนึง = เร็วขึ้น 2 เท่า
    [Tooltip("เปิดใช้ Typewriter Effect")]
    public bool enableTypewriterEffect = true; // ปิดได้ถ้าต้องการแสดงทันที
    [Tooltip("จำนวนตัวอักษรต่อ frame (เพิ่ม = เร็วขึ้น)")]
    public int charactersPerFrame = 2; // แสดง 2 ตัวต่อครั้งแทน 1 ตัว
    private bool isTyping = false;

    [Header("Text Layout Settings")]
    [Tooltip("เปิดใช้ Auto Size เมื่อข้อความยาวเกิน")]
    public bool useAutoSize = true;
    [Tooltip("ขนาดตัวอักษรขั้นต่ำ")]
    public float minFontSize = 12f;
    [Tooltip("ขนาดตัวอักษรขั้นสูงสุด")]
    public float maxFontSize = 24f;
    
    [Header("Loading Settings")]
    [Tooltip("ข้อความแสดงขณะโหลด")]
    public string loadingText = "กำลังวิเคราะห์ข้อมูล...";
    [Tooltip("เวลารอสูงสุด (วินาที) ก่อนแสดงคำแนะนำ")]
    public float maxWaitTime = 2f; // ลดจาก 4 เป็น 2 วินาที

    [Header("Game Recommendations")]
    [Tooltip("ชื่อเกมที่แนะนำสำหรับแต่ละส่วน")]
    public string[] recommendedGames = new string[]
    {
        "กำแพงหรรษา หรือ ท้าสมองประลองควิซ",
        "ท้าสมองประลองควิซ",
        "ท้าสมองประลองควิซ หรือ กำแพงหรรษา หรือ มือปราบยุงลาย",
        "กำแพงหรรษา",
        "สนุกกับเงา"
    };

    private int[] point;
    private string[] partNames = new string[]
    {
        "ด้านร่างกายท่อนบน",
        "ด้านร่างกายส่วนล่าง",
        "ด้านความคล่องแคล่ว",
        "ด้านความยืดหยุ่น",
        "ด้านมือกับสายตา"
    };

    // ข้อความเปิด (สุ่มได้)
    private string[] openingMessages = new string[]
    {
        "มาดูกันว่าวันนี้น้องเล่นเกมอะไรดีนะ!",
        "เอาล่ะ! มาดูว่าควรพัฒนาด้านไหนต่อดี",
        "ดูสถิติของน้องแล้ว เรามีเกมแนะนำเลย!",
        "พร้อมจะเล่นเกมใหม่แล้วใช่มั้ย? มาดูกัน!"
    };

    // ข้อความสำหรับแต่ละด้าน (แบบสั้นกระชับ)
    private string[][] partSpecificMessages = new string[][]
    {
        // Upper Body
        new string[]
        {
            "คะแนนท่อนบนของน้องต่ำหน่อยนะ ลองเล่นมือปราบยุงลาย หรือ กำแพงหรรษาดูไหม!",
            "ท่อนบนยังต้องพัฒนาอีกนะ ลองเล่นกำแพงหรรษาดูสิ!"
        },
        // Lower Body
        new string[]
        {
            "คะแนนส่วนล่างยังน้อยอยู่นะ ลองเล่นกำแพงหรรษา หรือ ท้าสมองประลองควิซดูสิ!",
            "ส่วนล่างยังต้องพัฒนาอีกนะ ลองเล่นท้าสมองประลองควิซดูไหม?"
        },
        // Agility
        new string[]
        {
            "คะแนนความคล่องแคล่วต่ำนะ แนะนำให้เล่นมือปราบยุงลาย หรือ ท้าสมองประลองควิซ!",
            "ยังไม่ค่อยคล่องแคล่วนะ ลองเล่นมือปราบยุงลายดูสิ!"
        },
        // Flexibility
        new string[]
        {
            "คะแนนความยืดหยุ่นต่ำนะ ต้องเล่นกำแพงหรรษาเสริมด่วน!",
            "ความยืดหยุ่นยังต้องพัฒนาอีกนะ ลองเล่นกำแพงหรรษาดูสิ!"
        },
        // Hand-Eye
        new string[]
        {
            "คะแนนมือกับสายตาต่ำนะ ลองเล่นเกมสนุกกับเงาดูสิ!",
            "ประสานมือกับตายังไม่ดีนะ ลองเล่นสนุกกับเงาดูไหม?"
        }
    };

    // ข้อความสำหรับหลายด้านที่ต่ำเท่ากัน
    private string[] multipleWeaknessMessages = new string[]
    {
        "คะแนนในหลายๆด้านยังขาดอยู่นะ ลองเล่นเกมกำแพงหรรษา หรือไม่ก็ มือปราบยุงลายดูสิ! เกมเหล่านี้จะช่วยพัฒนาหลายๆด้านไปพร้อมกัน",
        "ดูเหมือนหลายๆด้านของน้องยังต้องพัฒนาอีกนะ แนะนำให้เล่นท้าสมองประลองควิซ หรือ กำแพงหรรษาดูไหม?"
    };

    // ข้อความสำหรับคะแนนเท่ากันทุกด้าน
    private string[] balancedMessages = new string[]
    {
        "เยี่ยมเลย! คะแนนทุกด้านของน้องสมดุลกันดีมาก น้องสามารถเลือกเล่นเกมไหนก็ได้ตามที่ชอบเลย!",
        "ดีมาก! ทุกด้านของน้องพัฒนาไปพร้อมๆกัน ลองเลือกเกมที่สนุกที่สุดมาเล่นดูสิ!",
        "สุดยอด! คะแนนของน้องดีทุกด้านเลย เลือกเกมที่อยากเล่นได้เลยนะ!"
    };

    // ข้อความปิดท้าย
    private string[] closingMessages = new string[]
    {
        "มาลองเล่นกันเถอะ! พร้อมจะสนุกและแข็งแรงขึ้นแล้วใช่มั้ย?",
        "เล่นสนุกๆนะ! และอย่าลืมพยายามให้เต็มที่!",
        "ไปเล่นกันเลย! น้องทำได้แน่นอน!",
        "พร้อมแล้วใช่มั้ย? มาเริ่มเกมกันเถอะ!"
    };

    void Start()
    {
        point = new int[5];
        
        // ตั้งค่า Text Layout
        SetupTextLayout();
        
        // แสดงข้อความโหลด
        if (dialogueadvisor != null)
        {
            dialogueadvisor.text = loadingText;
        }
        
        StartCoroutine(WaitAndUpdateAdvice());
    }

    void SetupTextLayout()
    {
        if (dialogueadvisor == null) return;
        
        // เปิด Text Wrapping
        dialogueadvisor.enableWordWrapping = true;
        
        // ตั้งค่า Overflow
        dialogueadvisor.overflowMode = TextOverflowModes.Ellipsis; // หรือ Truncate
        
        // เปิด Auto Size (ถ้าต้องการ)
        if (useAutoSize)
        {
            dialogueadvisor.enableAutoSizing = true;
            dialogueadvisor.fontSizeMin = minFontSize;
            dialogueadvisor.fontSizeMax = maxFontSize;
        }
        
        Debug.Log("✅ Text Layout configured");
    }

    IEnumerator WaitAndUpdateAdvice()
    {
        float elapsedTime = 0f;
        
        // รอจนกว่าข้อมูลจะพร้อม หรือ timeout
        while (elapsedTime < maxWaitTime)
        {
            // ✅ เช็คว่า DashboardController โหลดข้อมูลเสร็จแล้วหรือยัง
            if (dashboardController != null && 
                dashboardController.part != null && 
                dashboardController.part.Length == 5)
            {
                // ตรวจสอบว่ามีข้อมูลจริง (ไม่ใช่ 0 ทั้งหมด)
                bool hasData = false;
                for (int i = 0; i < 5; i++)
                {
                    if (dashboardController.part[i] != 0)
                    {
                        hasData = true;
                        break;
                    }
                }
                
                if (hasData)
                {
                    Debug.Log($"✅ ข้อมูลพร้อมแล้วที่ {elapsedTime:F2} วินาที");
                    break;
                }
            }
            
            elapsedTime += 0.1f;
            yield return new WaitForSeconds(0.1f); // เช็คทุก 0.1 วินาที
        }
        
        if (elapsedTime >= maxWaitTime)
        {
            Debug.LogWarning($"⚠️ Timeout: รอข้อมูลเกิน {maxWaitTime} วินาที");
        }
        
        UpdatePointsAndAdvice();
    }

    public void UpdatePointsAndAdvice()
    {
        if (dashboardController == null)
        {
            Debug.LogError("❌ DashboardController reference is missing!");
            StartCoroutine(TypeText("ไม่สามารถโหลดข้อมูลได้ กรุณาลองใหม่อีกครั้ง"));
            return;
        }

        for (int i = 0; i < 5; i++)
        {
            point[i] = dashboardController.part[i];
        }

        Debug.Log($"📊 Points loaded: [{string.Join(", ", point)}]");
        GenerateAdvice();
    }

    void GenerateAdvice()
    {
        int minScore = point.Min();
        
        var lowestIndices = point
            .Select((score, index) => new { score, index })
            .Where(x => x.score == minScore)
            .Select(x => x.index)
            .ToList();

        string fullMessage = GenerateAdviceText(lowestIndices, minScore);
        
        // เริ่ม Typewriter Effect
        StartCoroutine(TypeText(fullMessage));
        
        Debug.Log($"💬 Advice generated: {fullMessage}");
    }

    // 🎬 Typewriter Effect Animation (ปรับปรุงให้เร็วขึ้น)
    IEnumerator TypeText(string textToType)
    {
        if (isTyping)
        {
            StopAllCoroutines();
        }
        
        isTyping = true;
        dialogueadvisor.text = "";
        
        // ถ้าปิด Typewriter Effect ให้แสดงทั้งหมดเลย
        if (!enableTypewriterEffect)
        {
            dialogueadvisor.text = textToType;
            isTyping = false;
            yield break;
        }
        
        // แสดงทีละหลายตัวอักษรเพื่อความเร็ว
        char[] characters = textToType.ToCharArray();
        int currentIndex = 0;
        
        while (currentIndex < characters.Length)
        {
            // แสดงหลายตัวอักษรต่อครั้ง
            int charsToAdd = Mathf.Min(charactersPerFrame, characters.Length - currentIndex);
            
            for (int i = 0; i < charsToAdd; i++)
            {
                dialogueadvisor.text += characters[currentIndex];
                currentIndex++;
            }
            
            yield return new WaitForSeconds(typingSpeed);
        }
        
        isTyping = false;
    }

    string GenerateAdviceText(System.Collections.Generic.List<int> lowestIndices, int minScore)
    {
        if (lowestIndices.Count == 0)
        {
            return "ไม่พบข้อมูล กรุณาลองใหม่อีกครั้ง";
        }

        string opening = openingMessages[Random.Range(0, openingMessages.Length)];
        string closing = closingMessages[Random.Range(0, closingMessages.Length)];
        string mainContent = "";

        // กรณีคะแนนเท่ากันทุกส่วน
        if (lowestIndices.Count == 5)
        {
            mainContent = balancedMessages[Random.Range(0, balancedMessages.Length)];
            return $"{opening}\n\n{mainContent}\n\n{closing}";
        }

        // กรณีมีหลายส่วนที่คะแนนต่ำสุดเท่ากัน
        if (lowestIndices.Count > 1)
        {
            mainContent = multipleWeaknessMessages[Random.Range(0, multipleWeaknessMessages.Length)];
            
            // เพิ่มรายละเอียดว่าด้านไหนบ้าง
            string weakParts = "";
            for (int i = 0; i < lowestIndices.Count; i++)
            {
                weakParts += partNames[lowestIndices[i]];
                if (i < lowestIndices.Count - 1)
                {
                    weakParts += ", ";
                }
            }
            
            return $"{opening}\n\nน้องมีคะแนน {minScore} คะแนน ใน: {weakParts}\n\n{mainContent}\n\n{closing}";
        }

        // กรณีมีเพียงส่วนเดียวที่คะแนนต่ำสุด
        int weakestPart = lowestIndices[0];
        string[] possibleMessages = partSpecificMessages[weakestPart];
        mainContent = possibleMessages[Random.Range(0, possibleMessages.Length)];

        return $"{opening}\n\n{mainContent}\n\n{closing}";
    }

    public void OnDateChanged()
    {
        StartCoroutine(WaitAndUpdateAdvice());
    }

    public void ForceUpdateAdvice()
    {
        UpdatePointsAndAdvice();
    }

    // สำหรับกดข้าม Animation
    public void SkipTyping()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            isTyping = false;
            // แสดงข้อความทั้งหมดทันที (ถ้ามีการเก็บไว้)
            Debug.Log("⏭️ ข้ามการพิมพ์");
        }
    }
    
    // เรียกจาก Button เพื่อโหลดข้อมูลใหม่ทันที
    public void RefreshAdvice()
    {
        if (dialogueadvisor != null)
        {
            dialogueadvisor.text = loadingText;
        }
        StartCoroutine(WaitAndUpdateAdvice());
    }
}