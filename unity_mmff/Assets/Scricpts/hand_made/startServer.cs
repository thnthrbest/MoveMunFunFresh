using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Diagnostics;
using TMPro;

public class startServer : MonoBehaviour
{
    [SerializeField] private bool CanStart = true;
    [SerializeField] private string ScriptFolder = "";
    [SerializeField] private string ScriptName = "";
    [SerializeField] private int delayForStartServer;
    [SerializeField] private Dec dec;
    [SerializeField] private CountdownTimer CountdownTimer;
    [SerializeField] private sendstate sendstate;
    [SerializeField] private manange manange;
    [SerializeField] private UDPReceive UDPReceive;
    [SerializeField] private TextMeshProUGUI score;
    [SerializeField] private AudioSource sound_correct;
    [SerializeField] private AudioSource sound_countdown;
    public RawImage loading;
    public RawImage correct;
    public RawImage[] images = new RawImage[4];

    int temp=0;

    IEnumerator StartServer()
    {
        Process.Start(ScriptFolder + ScriptName + ".vbs");
        UnityEngine.Debug.LogWarning("Server Has Started");
        yield return new WaitForSeconds(delayForStartServer);
        if (dec != null)
        {
            dec.enabled = true;
            yield return new WaitForSeconds(3f);
            StartCoroutine(re());
            yield return new WaitForSeconds(3f);
            loading.gameObject.SetActive(false);
            yield return new WaitForSeconds(1f);
            UnityEngine.Debug.Log("3");
            images[3].gameObject.SetActive(true);
            sound_countdown.Play();
            yield return new WaitForSeconds(1f);
            images[3].gameObject.SetActive(false);
            images[2].gameObject.SetActive(true);
            UnityEngine.Debug.Log("2");
            yield return new WaitForSeconds(1f);
            images[2].gameObject.SetActive(false);
            images[1].gameObject.SetActive(true);
            UnityEngine.Debug.Log("1");
            yield return new WaitForSeconds(1f);
            images[1].gameObject.SetActive(false);
            images[0].gameObject.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            images[0].gameObject.SetActive(false);
            StartCoroutine(CountdownTimer.StartCountdown());
            //UnityEngine.Debug.Log("send");
        }
    }
    IEnumerator re()
    {
        sendstate.send();
        correct.gameObject.SetActive(false);
        yield return new WaitForSeconds(2f);
        StartCoroutine(manange.change_img(UDPReceive.a[0]));
        yield return new WaitForSeconds(6f);
        while(true){
            int value = Random.Range(4,7);
            if(temp != 0)sendstate.send();
            yield return new WaitForSeconds(1f);
            StartCoroutine(manange.change_img(UDPReceive.a[0]));
            yield return new WaitForSeconds(value);
            UnityEngine.Debug.Log("score :" + temp + " value :"+value);
            temp++;
            score.text = temp.ToString();
            correct.gameObject.SetActive(true);
            sound_correct.Play();
            yield return new WaitForSeconds(1f);
            correct.gameObject.SetActive(false);
        }
    }
    public void ServerStart()
    {
        if (CanStart)
        {
            StartCoroutine(StartServer());
        }
    }

    private void Awake()
    {

        ServerStart();
    }
}

