using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using UnityEngine.UI;

public class StartServer : MonoBehaviour
{
    // Start is called before the first frame update
    public Dec dec;
    public GameObject cam;
    public GameObject sever;
    public void Start()
    {

        StartCoroutine(StartUP());
        

    }

    IEnumerator StartUP(){ //ใช้สำหรับหน่วงเวลา
        Process.Start("C:/MyMediapipe/run.bat");
        yield return new WaitForSeconds(10);
        dec.enabled = true;
        yield return new WaitForSeconds(3);
        cam.SetActive(true);
        
    }
    

}
