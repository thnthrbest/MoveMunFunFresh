using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class StartServer : MonoBehaviour
{
    // Start is called before the first frame update
    public DecSeen dec;
    public void Start()
    {

        StartCoroutine(StartUP());
        

    }

    IEnumerator StartUP(){ //ใช้สำหรับหน่วงเวลา
        Process.Start("D:/MoveMunFunFresh/MyMediapipe/run.bat");
        yield return new WaitForSeconds(5);
        dec.enabled = true;
        
    }
    

}
