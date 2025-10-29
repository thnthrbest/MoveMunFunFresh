using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Diagnostics;
using TMPro;

public class Startserver : MonoBehaviour
{
    [SerializeField] private bool CanStart = true;
    [SerializeField] private string ScriptFolder = "";
    [SerializeField] private string ScriptName = "";
    [SerializeField] private int delayForStartServer;
    [SerializeField] private Sendstate sendstate;
    //public RawImage loading;

    int temp=0;

    IEnumerator StartServer()
    {
        Process.Start(ScriptFolder + ScriptName + ".vbs");
        UnityEngine.Debug.LogWarning("Server Has Started");
        yield return new WaitForSeconds(delayForStartServer);
        StartCoroutine(re());
        
    }
    IEnumerator re()
    {
        sendstate.send();
        yield return new WaitForSeconds(delayForStartServer);
        
        
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

