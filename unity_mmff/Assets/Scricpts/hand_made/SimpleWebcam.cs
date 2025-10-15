using UnityEngine;
using UnityEngine.UI;

public class SimpleWebcam : MonoBehaviour
{
    public RawImage wait;   // UI ที่ใช้แสดงภาพจากกล้อง
    private WebCamTexture webcamTexture;

    void Start()
    {
        try
        {
            // ตรวจสอบอุปกรณ์กล้องทั้งหมด
            WebCamDevice[] devices = WebCamTexture.devices;

            // ใช้กล้องตัวแรก
            webcamTexture = new WebCamTexture(devices[1].name);
            wait.texture = webcamTexture;
            wait.material.mainTexture = webcamTexture;

            webcamTexture.Play();
        }
        catch (System.Exception e)
        {
            Debug.LogError("เกิดข้อผิดพลาดในการเปิดกล้อง: " + e.Message);
        }
    }

    void OnDestroy()
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            webcamTexture.Stop();
        }
    }
}
