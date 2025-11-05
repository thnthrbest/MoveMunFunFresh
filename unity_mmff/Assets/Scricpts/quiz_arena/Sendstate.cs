using UnityEngine.UI;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using System.Collections;
public class Sendstate : MonoBehaviour
{

    public bool isDetec;
    [SerializeField] private uDPSender uDPSender;
    [SerializeField] private main main;
    [SerializeField] private uDPReceive uDPReceive;
    [SerializeField] private CountdownTime CountdownTime;
    public RawImage[] images = new RawImage[4];
    public RawImage loading;
    [SerializeField] private AudioSource sound_countdown;

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            send();
        }
    }
    public void send()
    {
        Debug.Log("Wait for send naaaa");
        uDPSender.Sender(uDPSender.num);
        main.enabled = true;
        uDPReceive.enabled = true;
        //StartCoroutine(manange.start_shadow());
    }
    
    public IEnumerator sss()
    {
        Debug.Log("Wait for send");
        yield return new WaitForSeconds(2f);
        Debug.Log("Wait 3 sec complete");
        yield return new WaitForSeconds(2f);
        Debug.Log("Wait 2 sec complete");
        yield return new WaitForSeconds(4f);
        Debug.Log("Wait 1 sec complete");
        yield return new WaitForSeconds(10f);
        Debug.Log("Wait 0 sec complete");
        send();

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
        StartCoroutine(CountdownTime.StartCountdown());
        
    }
    void Start()
    {
        StartCoroutine(sss());
    }
}