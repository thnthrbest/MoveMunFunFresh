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

    [Header("Child Info UI")]
    public TextMeshProUGUI childNameText;
    public TextMeshProUGUI childNicknameText;
    public TextMeshProUGUI childWeightText;
    public TextMeshProUGUI childHeightText;
    public TextMeshProUGUI childAgeText;

    [Header("DatePicker Reference")]
    public DatePicker datePicker;
    
    [Header("Data Arrays")]
    public string[] posReturn;
    public int[] part = new int[5];

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

        // ⭐ รีเซ็ตวันที่เป็นวันนี้ทุกครั้งที่เข้า Dashboard
        ResetDateToToday();

        InitializeChart();

        // Subscribe to DatePicker event
        if (datePicker != null)
        {
            datePicker.OnDateChanged.AddListener(RefreshDataWithInvoke);
        }

        // โหลดข้อมูลเด็กและข้อมูลกิจกรรม
        LoadChildInfo();
        RefreshData();
    }

    void ResetDateToToday()
    {
        string todayDate = DateTime.Now.ToString("yyyy-MM-dd");
        PlayerPrefs.SetString("selected_date", todayDate);
        PlayerPrefs.Save();
        
        // อัปเดต DatePicker UI ให้แสดงวันนี้
        if (datePicker != null)
        {
            datePicker.SetDate(DateTime.Now);
        }
        
        Debug.Log("✓ Date reset to today: " + todayDate);
    }

    void RefreshDataWithInvoke()
    {
        Invoke(nameof(RefreshData), 3f);
    }
    
    void InitializeChart()
    {
        if (barchart == null)
        {
            Debug.LogError("❌ BarChart reference is null!");
            return;
        }

        Debug.Log("📊 Initializing chart...");

        barchart.RemoveData();

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

        var serie = barchart.AddSerie<Bar>("Parts");
        serie.barWidth = 0.6f;

        Debug.Log("Adding X-Axis labels:");
        foreach (string label in partLabels)
        {
            barchart.AddXAxisData(label);
            Debug.Log($"  - {label}");
        }

        for (int i = 0; i < 5; i++)
        {
            barchart.AddData(0, 0);
        }

        barchart.RefreshChart();
        isChartInitialized = true;

        Debug.Log("✓ Chart initialized successfully!");
    }

    // ⭐ ฟังก์ชันใหม่: โหลดข้อมูลเด็ก
    public void LoadChildInfo()
    {
        StartCoroutine(GetChildInfo());
    }

    IEnumerator GetChildInfo()
    {
        string url = "http://localhost/mmff_php/childinformation.php";
        WWWForm form = new WWWForm();
        form.AddField("child_id", child_id);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error fetching child info: " + www.error);
                yield break;
            }
            else
            {
                string response = www.downloadHandler.text.Trim();
                Debug.Log("Child Info Response: " + response);

                if (response == "0")
                {
                    Debug.LogWarning("No child data found!");
                    yield break;
                }

                // แยกข้อมูล: name:nickname:weight:height:age
                string[] childData = response.Split(':');

                if (childData.Length >= 5)
                {
                    // แสดงข้อมูลบน UI
                    if (childNameText != null)
                        childNameText.text = $"ชื่อ: {childData[1]}";

                    if (childNicknameText != null)
                        childNicknameText.text = $"ชื่อเล่น: {childData[2]}";

                    if (childWeightText != null)
                        childWeightText.text = $"น้ำหนัก: {childData[3]} kg";

                    if (childHeightText != null)
                        childHeightText.text = $"ส่วนสูง: {childData[4]} cm";

                    if (childAgeText != null)
                        childAgeText.text = $"อายุ: {childData[5]} ปี";

                    Debug.Log("✓ Child info displayed successfully!");
                }
                else
                {
                    Debug.LogError("Invalid child data format!");
                }
            }
        }
    }

    public void RefreshData()
    {
        StartCoroutine(GetDataPlay());
    }

    public IEnumerator GetDataPlay()
    {
        string url = "http://localhost/mmff_php/GetDataPlay.php";
        WWWForm form = new WWWForm();
        form.AddField("child_id", child_id);
        form.AddField("game_id", game_id);
        form.AddField("score", score);

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

                posReturn = response.Split(':');
                Debug.Log("Split into " + posReturn.Length + " parts");

                for (int i = 0; i < posReturn.Length; i++)
                {
                    Debug.Log($"posReturn[{i}] = '{posReturn[i]}'");
                }

                for (int i = 0; i < 5; i++)
                {
                    if (i < posReturn.Length)
                    {
                        string valueStr = posReturn[i].Trim();
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