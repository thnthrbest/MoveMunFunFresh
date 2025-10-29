//ไม่ใช่

using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine.InputSystem;
public class Sendstate : MonoBehaviour
{

    public bool isDetec;
    [SerializeField] private uDPSender uDPSender;
    [SerializeField] private main main;
    [SerializeField] private uDPReceive uDPReceive;

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            send();
        }
    }
    public void send(){
        uDPSender.Sender(uDPSender.num);
        main.enabled = true;
        uDPReceive.enabled = true;
        //StartCoroutine(manange.start_shadow());
    }



}