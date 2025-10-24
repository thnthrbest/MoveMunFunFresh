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


    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            uDPSender.Sender(isDetec.ToString());
        }
    }
    public void send(){
        uDPSender.Sender(isDetec.ToString());
        //StartCoroutine(manange.start_shadow());
    }



}