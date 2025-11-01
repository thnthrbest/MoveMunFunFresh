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
    public string[] partLabels = new string[]
    {
        "Upper_Body",
        "Lower_Body",
        "Agility",
        "Flexibility",
        "Hand-Eye"
    };

    private bool isChartInitialized = false;
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

        InitializeChart();

        // ⭐ Subscribe to DatePicker event
        if (datePicker != null)
        {
            datePicker.OnDateChanged.AddListener(RefreshDataWithInvoke);
        }

        // โหลดข้อมูลครั้งแรก
        RefreshData();
    }

    void RefreshDataWithInvoke()
    {
        Invoke(nameof(RefreshData), 5f); // รอ 5 วินาที
    }
    
    void InitializeChart()
    {
        if (barchart == null)
        {
            Debug.LogError("❌ BarChart reference is null!");
            return;
        }

        Debug.Log("📊 Initializing chart...");

        // Clear ข้อมูลเก่าทั้งหมด
        barchart.RemoveData();

        // Setup Chart Components
        var title = barchart.EnsureChartComponent<Title>();
        title.text = "กราฟกิจกรรมทางกาย";
        title.show = true;

        var xAxis = barchart.EnsureChartComponent<XAxis>();
        xAxis.show = true;
        xAxis.type = Axis.AxisType.Category;

        var yAxis = barchart.EnsureChartComponent<YAxis>();
        yAxis.show = true;
        yAxis.type = Axis.AxisType.Value;
        yAxis.minMaxType = Axis.AxisMinMaxType.Default;

        var tooltip = barchart.EnsureChartComponent<Tooltip>();
        tooltip.show = true;

        // เพิ่ม Serie
        var serie = barchart.AddSerie<Bar>("Parts");
        serie.barWidth = 0.6f;

        // ⭐ เพิ่ม X-Axis Labels
        Debug.Log("Adding X-Axis labels:");
        foreach (string label in partLabels)
        {
            barchart.AddXAxisData(label);
            Debug.Log($"  - {label}");
        }

        // เพิ่มข้อมูลเริ่มต้น (0 ทั้งหมด)
        for (int i = 0; i < 5; i++)
        {
            barchart.AddData(0, 0);
        }

        barchart.RefreshChart();
        isChartInitialized = true;

        Debug.Log("✓ Chart initialized successfully!");
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
                UpdateChartData();
            }
        }
    }

    void UpdateChartData()
    {
        if (barchart == null)
        {
            Debug.LogError("❌ BarChart reference is null!");
            return;
        }

        if (!isChartInitialized)
        {
            Debug.LogWarning("Chart not initialized, initializing now...");
            InitializeChart();
        }

        Debug.Log("📊 Updating chart data:");

        // ⭐ ใช้ UpdateData() แทนการสร้างใหม่
        for (int i = 0; i < 5; i++)
        {
            barchart.UpdateData(0, i, part[i]);
            Debug.Log($"  {partLabels[i]}: {part[i]}");
        }

        barchart.RefreshChart();
        Debug.Log("✓ Chart updated successfully!");
    }

    void OnDestroy()
    {
        if (datePicker != null)
        {
            datePicker.OnDateChanged.RemoveListener(RefreshData);
        }
    }
    
}