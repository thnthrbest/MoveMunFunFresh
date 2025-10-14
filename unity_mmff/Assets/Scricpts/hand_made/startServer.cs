using System.Collections;
using System.Collections.Generic;
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
    int temp=0;

    IEnumerator StartServer()
    {
        Process.Start(ScriptFolder + ScriptName + ".vbs");
        UnityEngine.Debug.LogWarning("Server Has Started");
        yield return new WaitForSeconds(delayForStartServer);
        if (dec != null)
        {
            dec.enabled = true;
            CountdownTimer.enabled = true;
            yield return new WaitForSeconds(3);
            StartCoroutine(re());
            //UnityEngine.Debug.Log("send");
        }
    }
    IEnumerator re()
    {
        while(true){

            sendstate.send();
            yield return new WaitForSeconds(10f);
            StartCoroutine(manange.change_img(UDPReceive.a[0]));
            temp++;
            score.text = temp.ToString();
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

