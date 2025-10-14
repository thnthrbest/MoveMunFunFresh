using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections;


public class manange : MonoBehaviour
{

    public RawImage animal_pic;   // Drag RawImage จาก Inspector มาใส่
    public string imagePath = "D:/Images/myPic.jpg"; // ตำแหน่งไฟล์

    public CountdownTimer CountdownTimer;

    
    public  IEnumerator change_img(string a)
    {
        yield return new WaitForSeconds(1f);

        imagePath = "D:/GitHub/MoveMunFunFresh/unity_mmff/Assets/img_handmade/" + a + ".png";

        // โหลด byte[] จากไฟล์
        byte[] fileData = File.ReadAllBytes(imagePath);

        // สร้าง Texture2D ว่าง
        Texture2D tex = new Texture2D(2, 2);

        // แปลง byte[] เป็นรูป
        tex.LoadImage(fileData);

        // เอาไปใส่ใน RawImage
        animal_pic.texture = tex;

        yield return null;
    }
}
//rabbit,elephent,snail,dog,deer,cow,crab,bird