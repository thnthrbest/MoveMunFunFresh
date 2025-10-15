using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    public float timeRemaining = 60f;   // ตั้งเวลาเริ่มต้น 60 วิ
    public TextMeshProUGUI timerText;              // ถ้ามี Text UI ให้ผูกใน Inspector
    public bool isRunning = false;
    private int lastSentSecond = -1;
    int temp=0;

    void Start()
    {
        StartCoroutine(StartCountdown());
    }

    void Update(){
        timeRemaining -= Time.deltaTime;
        int currentSecond = Mathf.FloorToInt(timeRemaining);

        if (currentSecond % 10 == 0 && currentSecond != lastSentSecond && currentSecond != 60)
        {
            lastSentSecond = currentSecond;
        }
    
    }

    IEnumerator StartCountdown()
    {
        yield return new WaitForSeconds(3f);
        while (timeRemaining > 0)
        {
            
            timeRemaining -= Time.deltaTime;

            if (timerText != null)
                timerText.text = Mathf.CeilToInt(timeRemaining).ToString();
            else
                Debug.Log(Mathf.CeilToInt(timeRemaining));

            yield return null;
        }

        isRunning = false;
        Debug.Log("Time's up!");
    }
}
