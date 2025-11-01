using UnityEngine.UIElements;
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using XCharts.Runtime;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
public class DashboardController : MonoBehaviour
{
    [Header("Child Setting")]
    [SerializeField] private string ChildIdKey = "child_id";
    private string child_id;
    private int score, game_id;

    [Header("DatePicker Reference")]
    public DatePicker datePicker;  // ⭐ เพิ่ม Reference
    
    [Header("Data Arrays")]
    public string[] posReturn; // ไม่ต้อง initialize
    public int[] part = new int[5]; // ต้อง initialize ให้มี 5 ช่อง

    [Header("Chart Reference")]
    public BarChart barchart;
    void Start()
    {
        // Initialize
        if (part == null || part.Length != 5)
        {
            part = new int[5];
        }
        
        child_id = PlayerPrefs.GetString("child_id", "Null");
        score = PlayerPrefs.GetInt("Score");
        game_id = PlayerPrefs.GetInt("game_id");

        Debug.Log($"Starting with child_id: {child_id}, game_id: {game_id}, score: {score}");

        // ⭐ Subscribe to DatePicker event
        if (datePicker != null)
        {
            datePicker.OnDateChanged.AddListener(RefreshData);
        }

        // โหลดข้อมูลครั้งแรก
        RefreshData();
    }

    public void RefreshData()
    {
        StartCoroutine(GetDataPlay());
    }
    public IEnumerator GetDataPlay()
    {
        string url = "http://localhost/mmff/GetDataPlay.php";
        WWWForm form = new WWWForm();
        form.AddField("child_id", child_id);
        form.AddField("game_id", game_id);
        form.AddField("score", score);

        // ✨ เพิ่มการส่งวันที่จาก DatePicker
        string selectedDate = PlayerPrefs.GetString("selected_date", DateTime.Now.ToString("yyyy-MM-dd"));
        form.AddField("play_date", selectedDate);
        Debug.Log("Sending date: " + selectedDate);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error fetching data: " + www.error);
            }
            else
            {
                string response = www.downloadHandler.text.Trim();
                Debug.Log("Response received: " + response);

                // แยกข้อมูลด้วย :
                posReturn = response.Split(':');
                Debug.Log("Split into " + posReturn.Length + " parts");

                // แสดงข้อมูลที่แยกได้
                for (int i = 0; i < posReturn.Length; i++)
                {
                    Debug.Log($"posReturn[{i}] = '{posReturn[i]}'");
                }

                // Parse ข้อมูลอย่างปลอดภัย
                for (int i = 0; i < 5; i++)
                {
                    if (i < posReturn.Length) // เช็คว่า index ไม่เกิน
                    {
                        string valueStr = posReturn[i].Trim(); // ลบช่องว่าง
                        if (int.TryParse(valueStr, out int value))
                        {
                            part[i] = value;
                            Debug.Log($"✓ part[{i}] = {value}");
                        }
                        else
                        {
                            part[i] = 0;
                            Debug.LogWarning($"✗ Cannot parse '{valueStr}', set part[{i}] = 0");
                        }
                    }
                    else
                    {
                        part[i] = 0;
                        Debug.Log($"○ part[{i}] = 0 (no data)");
                    }
                }

                Debug.Log("Data parsing completed. Updating chart...");

                // Update chart หลังจาก parse ข้อมูลเสร็จ
                ChartUpdate();
            }
        }
    }

    public void ChartUpdate()
    {
        if (barchart == null)
        {
            Debug.LogError("BarChart reference is null! Please assign it in Inspector.");
            return;
        }

        Debug.Log("Starting chart update...");

        // ลบข้อมูลเก่า
        barchart.RemoveData();

        // เพิ่ม serie ถ้ายังไม่มี
        if (barchart.series.Count == 0)
        {
            Debug.Log("Adding new Bar serie...");
            barchart.AddSerie<Bar>("BarChart");
        }

        // เพิ่มข้อมูลทีละตัว
        Debug.Log("Adding data to chart:");
        for (int i = 0; i < 5; i++)
        {
            barchart.AddData(0, part[i]);
            Debug.Log($"  Data {i}: {part[i]}");
        }

        // Refresh chart
        barchart.RefreshChart();
        Debug.Log("✓ Chart updated successfully!");
    }
    
    void OnDestroy()
    {
        // ⭐ Unsubscribe เมื่อ destroy
        if (datePicker != null)
        {
            datePicker.OnDateChanged.RemoveListener(RefreshData);
        }
    }
}