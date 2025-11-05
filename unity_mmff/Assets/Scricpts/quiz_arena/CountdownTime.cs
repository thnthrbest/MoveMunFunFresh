using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class CountdownTime : MonoBehaviour
{
    public float timeRemaining = 10f;   // ตั้งเวลาเริ่มต้น 60 วิ
    public TextMeshProUGUI timerText; 
    public TextMeshProUGUI pointText; 
    public TextMeshProUGUI quiz_t;              // ถ้ามี Text UI ให้ผูกใน Inspector
    public bool isRunning = false;
    private int lastSentSecond = -1;
    int temp = 0;
    [SerializeField] private uDPReceive uDPReceive;
    [SerializeField] private AudioSource sound_correct;
    public GameObject o, l;


    string[] quiz = new string[100] {
    "2 + 3 = 5","ภาษาอังกฤษมีตัวอักษร 26 ตัว","1 สัปดาห์มี 7 วัน","พยัญชนะไทยมีทั้งหมด 44 ตัว","สีประจำวันจันทร์ คือสี เหลือง","สีประจำวันอาทิตย์ คือสีแดง","สีประจำวันอังคาร คือสีชมพู่","สีประจำวันพุธ คือสีเขียว","สีประจำวันพฤหัสบดี คือสีส้ม","สีประจำวันศุกร์ คือสีฟ้า","สีประจำวันเสาร์ คือสีม่วง","รุ้งกินน้ำมี 7 สี","สุนัขภาษาอังกฤษคือ Dog","เดือนที่ 1 คือ เดือนมกราคม","เดือนที่ 2 คือ เดือนกุมภาพันธ์","เดือนที่ 3 คือ เดือนมีนาคม","เดือนที่ 4 คือ เดือนเมษายน","เดือนที่ 5 คือ เดือนพฤษภาคม","เดือนที่ 6 คือ เดือนมิถุนายน","เดือนที่ 7 คือ เดือนกรกฎาคม","เดือนที่ 8 คือ เดือนสิงหาคม","เดือนที่ 9 คือ เดือนกันยายน","เดือนที่ 10 คือ เดือนตุลาคม","1 วันมี 24 ชั่วโมง","เดือนที่ 12 คือ เดือนธันวาคม","เดือนที่ 11 คือ เดือนพฤศจิกายน","White แปลว่าสีขาว","Green แปลว่าสีเขียว","Red แปลว่าสีแดง","Blue แปลว่าสีฟ้า","Yellow แปลว่าสีเหลือง","Black แปลว่าสีดำ","Dog แปลว่าสุนัข","Cat แปลว่าแมว","Bird แปลว่านก","Fish แปลว่าปลา","Apple แปลว่าผลไม้","Banana แปลว่ากล้วย","Grapes แปลว่าองุ่น","Orange แปลว่าส้ม","Pineapple แปลว่าสับปะรด","Water แปลว่าน้ำ","Milk แปลว่านม","Juice แปลว่าน้ำผลไม้","Bread แปลว่าขนมปัง","Rice แปลว่าข้าว","Egg แปลว่าไข่","Meat แปลว่าหมู","Chicken แปลว่าไก่","Vegetable แปลว่าผัก","Fruit แปลว่าผลไม้",
    "Black คือ สีชมพู","1 ปี มี 5 เดือน","เดือนที่ 1 คือ เดือนธันวาคม","Red คือ สีเหลือง","สีประจำวันจันทร์ คือสี ชมพู","สีประจำวันอาทิตย์ คือสีส้ม","สีประจำวันอังคาร คือสีเขียว","สีประจำวันพุธ คือสีฟ้า","สีประจำวันพฤหัสบดี คือสีแดง","สีประจำวันศุกร์ คือสีเขียว","พยัญชนะไทยมีทั้งหมด 26 ตัว","ภาษาอังกฤษมีตัวอักษร 44 ตัว","เดือนที่ 2 คือ เดือนเมษายน","เดือนที่ 3 คือ เดือนมิถุนายน","เดือนที่ 4 คือ เดือนกุมภาพันธ์","เดือนที่ 5 คือ เดือนกรกฎาคม","เดือนที่ 6 คือ เดือนมีนาคม","เดือนที่ 7 คือ เดือนพฤษภาคม","เดือนที่ 8 คือ เดือนกันยายน","เดือนที่ 9 คือ เดือนสิงหาคม","เดือนที่ 10 คือ เดือนพฤศจิกายน","เดือนที่ 12 คือ มกราคม","เดือนที่ 11 คือ เดือนตุลาคม","1 วันมี 12 ชั่วโมง","White แปลว่าสีดำ","Green แปลว่าสีแดง","Red แปลว่าสีฟ้า","Blue แปลว่าสีเหลือง","Yellow แปลว่าสีชมพู","Black แปลว่าสีขาว","Dog แปลว่าแมว","Cat แปลว่าสุนัข","Bird แปลว่าปลา","Fish แปลว่านก","Apple แปลว่ากล้วย","Banana แปลว่าผลไม้","Grapes แปลว่าส้ม","Orange แปลว่าองุ่น","Pineapple แปลว่าน้ำผลไม้","Water แปลว่านม","Milk แปลว่าน้ำ","Juice แปลว่าข้าว","Bread แปลว่าไข่","Rice แปลว่าขนมปัง","Egg แปลว่าหมู","Meat แปลว่าไก่","Chicken แปลว่าผัก","Vegetable แปลว่าผลไม้","Fruit แปลว่ากล้วย"
    };
    //string[] quiz = new string[1] {"2 + 3 = 5"};
    public IEnumerator loadquiz()
    {
        int sum = Random.Range(0, 100);
        while(true){
            if( sum != sum_bf)break;
            sum = Random.Range(0, 100);
        }
        quiz_t.text = quiz[sum];
        if(sum<50) temp = 0;
        else temp = 1;
        sum_bf = sum;
        yield return new WaitForSeconds(0.5f);
        
    }
    

    public IEnumerator StartCountdown()
    {
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(2f);
            pointText.text = (i + 1).ToString() + "/10";
            timeRemaining = 10f;
            
            while (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;

                if (timerText != null)
                    timerText.text = Mathf.CeilToInt(timeRemaining).ToString();
                else
                    Debug.Log(Mathf.CeilToInt(timeRemaining));
                yield return null;
            }
            StartCoroutine(call());
        }
        PlayerPrefs.SetString("game_id", "3");
        for(int i=0;i < 5;i++)
        {
            PlayerPrefs.SetInt($"score_{i}", 4);
        }
        SceneManager.LoadScene("MultiEndGame");
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
        o.SetActive(false);
        l.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(loadquiz());
    }
    void Start()
    {
        //StartCoroutine(StartCountdown());
        
        StartCoroutine(loadquiz());
    }

}
