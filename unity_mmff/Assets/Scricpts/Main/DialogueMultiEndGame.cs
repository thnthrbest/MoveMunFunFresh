using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueMultiEndGame : MonoBehaviour
{
    public MultiEndGame multiendgame;
    public int[] point;
    private int temp = 0;
    public TextMeshProUGUI Dialogue;

    [Header("Animation Settings")]
    public float typingSpeed = 0.05f; // ความเร็วในการพิมพ์ (วินาทีต่อตัวอักษร)
    private bool isTyping = false;
    private string fullText = "";

    // ข้อความตามคะแนนรวม
    private string[] dialogue50 = {
        "ยอดเยี่ยมมาก! คุณมีทักษะที่สมบูรณ์แบบ",
        "เก่งมากเลย! คุณเป็นมือโปรแล้ว"
    };
    
    private string[] dialogue40 = {
        "ดีมาก! คุณมีพัฒนาการที่น่าประทับใจ",
        "เยี่ยมเลย! อีกนิดเดียวก็เป็นมือโปร"
    };
    
    private string[] dialogue30 = {
        "ดีแล้ว! แต่ยังมีที่ต้องพัฒนาอีก",
        "พอใช้ได้ แต่ควรฝึกฝนเพิ่มเติม"
    };
    
    private string[] dialogue20 = {
        "พอใช้ คุณต้องฝึกฝนมากขึ้น",
        "ดีละ ลองฝึกเพิ่มนะ"
    };
    
    private string[] dialogue10 = {
        "ต้องพัฒนาอีกมาก ลองเล่นเกมฝึกฝนดูนะ",
        "พยายามต่อไปนะ ตอนนี้ถือเรามาถึงขั้นแรกแล้ว"
    };
    
    private string[] dialogueBelow10 = {
        "ยังต้องฝึกฝนมากๆ อย่าท้อนะ",
        "เริ่มต้นกันใหม่ ค่อยๆ พัฒนาไป"
    };

    // ข้อความแนะนำตาม point ที่น้อยที่สุด
    private string[] improvementSuggestions = {
        "คะแนนด้านด้านร่างกายท่อนบนน้อยกว่าอันอื่นลองเล่นมือปราบยุงลายหรือกำแพงหรรษาต่อดูไหม!",
        "คะแนนด้านร่างกานส่วนล่างยังน้อยอยู่ลองเล่นกำแพงหรรษาหรือท้าสมองประลองควิซดูสิ!",
        "คะแนนด้านความคล่องแคล่วน้อยกว่าอันอื่นแนะนำให้ลองเล่นมือปราบยุงลายหรือท้าสมองประลองควิซเป็นลำดับต่อไป!",
        "คะแนนด้านความยืดหยุ่นน้อยนะจำเป็นต้องเล่นกำแพงหรรษาเพื่อเสริมคะแนนส่วนนี้!",
        "คะแนนมือกับสายตาน้อยเว่าเพื่อนๆอันอื่นนะลองเล่นเกมสนุกกับเงาดูสิ!",
        "คะแนนในหลายๆด้านยังขาดอยู่ลองเล่นเกมกำแพงหรรษาหรือไม่ก็มือปราบยุงลายดูสิ!"
    };

    void Start()
    {
        point = new int[5];
        // ✨ รอให้ MultiEndGame โหลดข้อมูลเสร็จก่อน
        StartCoroutine(WaitForDataAndCalculate());
    }

    // ✨ รอให้ data พร้อมก่อนคำนวณ
    IEnumerator WaitForDataAndCalculate()
    {
        // รอจนกว่า data จะไม่เป็น null และมีครบ 5 ตัว
        while (multiendgame == null || 
            multiendgame.data == null || 
            multiendgame.data.Length < 5)
        {
            yield return null; // รอ 1 frame
        }

        // เพิ่มการรอเล็กน้อยเพื่อให้แน่ใจว่าข้อมูลพร้อม
        yield return new WaitForSeconds(0.1f);

        // ตอนนี้ data พร้อมแล้ว เริ่มคำนวณ
        CalculatePoint();
    }

    void CalculatePoint()
    {
        if (multiendgame != null && multiendgame.data != null)
        {
            for (int i = 0; i < 5; i++)
            {
                point[i] = int.Parse(multiendgame.data[i]);
            }
        }
        
        temp = 0;
        for (int i = 0; i < 5; i++)
        {
            temp += point[i];
        }

        ShowDialogue();
    }
    
    void ShowDialogue()
    {
        string messageX = GetDialogueByScore();
        string messageY = GetImprovementSuggestion();
        
        fullText = messageX + "\n\n" + messageY;
        
        // เริ่ม Animation
        StartCoroutine(TypeText(fullText));
    }
    
    // Typewriter Effect Animation
    IEnumerator TypeText(string textToType)
    {
        isTyping = true;
        Dialogue.text = "";
        
        foreach (char letter in textToType.ToCharArray())
        {
            Dialogue.text += letter;
            // รอตามความเร็วที่กำหนด
            yield return new WaitForSeconds(typingSpeed);
        }
        
        isTyping = false;
    }
    
    // หาข้อความตามคะแนนรวม (x)
    string GetDialogueByScore()
    {
        string[] selectedDialogues;
        
        if (temp >= 50)
        {
            selectedDialogues = dialogue50;
        }
        else if (temp >= 40)
        {
            selectedDialogues = dialogue40;
        }
        else if (temp >= 30)
        {
            selectedDialogues = dialogue30;
        }
        else if (temp >= 20)
        {
            selectedDialogues = dialogue20;
        }
        else if (temp >= 10)
        {
            selectedDialogues = dialogue10;
        }
        else
        {
            selectedDialogues = dialogueBelow10;
        }
        
        int randomIndex = Random.Range(0, selectedDialogues.Length);
        return selectedDialogues[randomIndex];
    }
    
    // หาข้อความแนะนำตาม point ที่น้อยที่สุด (y)
    string GetImprovementSuggestion()
    {
        int count = 0;
        int minIndex = 0;
        int minValue = point[0];

        for (int i = 0; i < 5; i++)
        {
            if (point[i] <= 0) count++;
        }
        if (count == 1 || count == 0)
        {
            for (int i = 1; i < point.Length; i++)
            {
                if (point[i] < minValue)
                {
                    minValue = point[i];
                    minIndex = i;
                }
            }
            return improvementSuggestions[minIndex];
        }
        else return improvementSuggestions[5];
    }
}
