using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class startServer : MonoBehaviour
{
    [SerializeField] private bool CanStart = true;
    [SerializeField] private string ScriptFolder = "";
    [SerializeField] private string ScriptName = "";
    [SerializeField] private int delayForStartServer;
    [SerializeField] private Dec dec;
    [SerializeField] private CountdownTimer CountdownTimer;
    [SerializeField] private sendstate sendstate;


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
            sendstate.send();
            //UnityEngine.Debug.Log("send");
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

