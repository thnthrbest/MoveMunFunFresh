using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class StartServer : MonoBehaviour
{
    // Start is called before the first frame update
    public DecSeen dec;
    public GameObject ls;
    public TimeCounter tc;
    public void Start()
    {

        StartCoroutine(StartUP());
    }

    IEnumerator StartUP(){ //ใช้สำหรับหน่วงเวลา
        Process.Start("D:/MoveMunFunFresh/MyMediapipe/run.bat");
        yield return new WaitForSeconds(8);
        dec.enabled = true;
        yield return new WaitForSeconds(2);
        ls.SetActive(false);
        tc.enabled = true;

        
    }
    

}
