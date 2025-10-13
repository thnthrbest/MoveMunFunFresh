using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine.InputSystem;
public class sendstate : MonoBehaviour
{

    public bool isDetec;
    [SerializeField] private UDPSender uDPSender;
    [SerializeField] private manange manange;


    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            uDPSender.Sender(isDetec.ToString());
            manange.play = isDetec;
        }
    }



}