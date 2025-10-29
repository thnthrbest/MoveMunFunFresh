using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Unity.VisualScripting;

public class CountdownTime : MonoBehaviour
{
    public float timeRemaining = 60f;   // ตั้งเวลาเริ่มต้น 60 วิ
    public TextMeshProUGUI timerText; 
    public TextMeshProUGUI quiz_t;              // ถ้ามี Text UI ให้ผูกใน Inspector
    public bool isRunning = false;
    private int lastSentSecond = -1;
    int temp = 0;
    [SerializeField] private uDPReceive uDPReceive;
    [SerializeField] private AudioSource sound_correct;
    public GameObject o,l;

    string[] quiz = new string[4] { "5 + 1 = 4 ?", "กล้วยที่สุกมีสีน้ำเงิน  ?", "สุนัขภาษาอังกฤษคือ Dog  ?", "รุ้งกินน้ำมี 7 สี ?" };
    int[] quiz_answer = new int[4] { 1, 1, 0, 0 };
    void Start()
    {
        //StartCoroutine(StartCountdown());
    }

    public IEnumerator StartCountdown()
    {
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
    int sum_bf= -1;
    public IEnumerator call()
    {
        if (temp == 0)
        {
            StartCoroutine(uDPReceive.game(true));
        }
        else if (temp == 1)
        {
            StartCoroutine(uDPReceive.game(false));
        }
        yield return new WaitForSeconds(1f);
        if (temp == 0) o.SetActive(true);
        else if (temp == 1) l.SetActive(true);
        sound_correct.Play();
        yield return new WaitForSeconds(1f);
        if (temp == 0) o.SetActive(false);
        if (temp == 1) l.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        int sum = Random.Range(0, 4);
        while (sum == sum_bf)
        {
            sum = Random.Range(0, 4);
        }
        quiz_t.text = quiz[sum];
        temp = quiz_answer[sum];
        sum_bf = sum;
        isRunning = false;
        
    }

}
