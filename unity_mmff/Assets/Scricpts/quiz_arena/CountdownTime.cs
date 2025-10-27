using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Unity.VisualScripting;

public class CountdownTime : MonoBehaviour
{
    public float timeRemaining = 60f;   // ตั้งเวลาเริ่มต้น 60 วิ
    public TextMeshProUGUI timerText; 
    public TextMeshProUGUI quiz;              // ถ้ามี Text UI ให้ผูกใน Inspector
    public bool isRunning = false;
    private int lastSentSecond = -1;
    int temp = 0;
    [SerializeField] private uDPReceive uDPReceive;
    [SerializeField] private AudioSource sound_correct;
    public RawImage correct;


    // void Update(){
    //     timeRemaining -= Time.deltaTime;
    //     int currentSecond = Mathf.FloorToInt(timeRemaining);

    //     if (currentSecond % 10 == 0 && currentSecond != lastSentSecond && currentSecond != 60)
    //     {
    //         lastSentSecond = currentSecond;
    //     }

    // }
    void Start()
    {
        StartCoroutine(StartCountdown());
    }

    public IEnumerator StartCountdown()
    {
        yield return new WaitForSeconds(2f);
        while (timeRemaining > 0)
        {
            if(Mathf.RoundToInt(timeRemaining) % 10 == 0 &&  Mathf.RoundToInt(timeRemaining) != 60 && isRunning == false)
            {
                Debug.Log("Send Time: " + Mathf.RoundToInt(timeRemaining));
                StartCoroutine(call());
                isRunning = true;
            }
            timeRemaining -= Time.deltaTime;

            if (timerText != null)
                timerText.text = Mathf.CeilToInt(timeRemaining).ToString();
            else
                Debug.Log(Mathf.CeilToInt(timeRemaining));
            yield return null;
        }

        //isRunning = false;
        Debug.Log("Time's up!");
    }
    
    public IEnumerator call()
    {
        yield return new WaitForSeconds(1f);
        if (temp == 0)
        {
            StartCoroutine(uDPReceive.game(true));
        }
        else if (temp == 1)
        {
            StartCoroutine(uDPReceive.game(false));
        }
        yield return new WaitForSeconds(2f);
        correct.gameObject.SetActive(true);
        sound_correct.Play();
        yield return new WaitForSeconds(1f);
        correct.gameObject.SetActive(false);
        yield return new WaitForSeconds(1f);
        quiz.text = "กล้วยที่สุกมีสีเขียวถูกต้องหรือผิด";
        isRunning = false;
    }

}
