using UnityEngine;
using TMPro; // 👈 1. ต้องมีสิ่งนี้สำหรับ TextMeshPro
using System.Collections;
using UnityEngine.UI; // 👈 2. ต้องมีสิ่งนี้สำหรับ Coroutine

[RequireComponent(typeof(AudioSource))] 
public class TimeCounter : MonoBehaviour
{
    [Header("UI Display")]
    [Tooltip("ลาก Text (TextMeshPro) ที่จะใช้แสดง 3, 2, 1")]
    public TextMeshProUGUI countdownText;

    [Header("Audio Clips")]
    [Tooltip("เสียงที่จะเล่น 'ครั้งเดียว' ตอนเริ่มนับ (3...)")]
    public AudioClip startSound; 

    [Tooltip("เสียงที่จะเล่น 'วนลูป' หลังจากนับเสร็จ")]
    public AudioClip loopSound;

    // --- ⭐️ 1. เพิ่มตัวแปรสำหรับสคริปต์ที่จะเปิด ⭐️ ---
    [Header("Actions After Countdown")]
    [Tooltip("ลาก 'สคริปต์' (เช่น Dec.cs) ที่ต้องการเปิดใช้งานหลังนับเสร็จ")]
    public MonoBehaviour scriptToEnable; // 👈 ใช้ MonoBehaviour จะยืดหยุ่นที่สุด

    public RawImage cam;
    public GameObject sev;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        
        // --- ⭐️ 2. ปิดสคริปต์เป้าหมายไว้ก่อน (ถ้ามันเปิดอยู่) ⭐️ ---
        // (เพื่อให้แน่ใจว่ามันจะเริ่มทำงานตอนนับเสร็จเท่านั้น)
        if (scriptToEnable != null)
        {
            scriptToEnable.enabled = false;
        }
        else
        {
            Debug.LogError("StartCountdown: ยังไม่ได้ลาก 'Script To Enable' มาใส่ใน Inspector!");
        }
    }

    void Start()
    {
        // 3. เรียก Coroutine ให้เริ่มทำงาน
        StartCoroutine(CountdownRoutine());
    }

    IEnumerator CountdownRoutine()
    {
        // --- 4. เริ่มนับ (Event 1) ---

        // ทำให้ Text 'เปิด' ก่อน
        countdownText.enabled = true;

        if (startSound != null)
        {
            audioSource.PlayOneShot(startSound); 
        }

        countdownText.text = "3";
        yield return new WaitForSeconds(1.0f); 

        countdownText.text = "2";
        yield return new WaitForSeconds(1.0f); 

        countdownText.text = "1";
        yield return new WaitForSeconds(1.0f); 

        // --- 5. นับเสร็จ! (Event 2) ---

        // ⭐️ 3. ปิดข้อความ (ตามที่คุณขอ) ⭐️
        countdownText.enabled = false; // 👈 ซ่อน Text

        // เล่นเสียง "วนลูป" (เหมือนเดิม)
        if (loopSound != null)
        {
            audioSource.clip = loopSound;   
            audioSource.loop = true;    
            audioSource.Play();       
        }

        // ⭐️ 4. เปิดสคริปต์อื่น (ตามที่คุณขอ) ⭐️
        if (scriptToEnable != null)
        {
            scriptToEnable.enabled = true; // 👈 เปิดการทำงานสคริปต์เป้าหมาย
            Debug.Log("เปิดการทำงานสคริปต์: " + scriptToEnable.GetType().Name);
        }
        if (cam != null)
        {
            cam.enabled = true; // 👈 เปิดการทำงานสคริปต์เป้าหมาย
            Debug.Log("เปิดการทำงานสคริปต์: " + cam.GetType().Name);
        }
        if (sev != null)
        {
            sev.SetActive(true);// 👈 เปิดการทำงานสคริปต์เป้าหมาย
            Debug.Log("เปิดการทำงานสคริปต์: " + sev.GetType().Name);
        }
    }
}