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
    [Header("Child Settings")]
    [SerializeField] private string childIdKey = "Child_id"; // Key จาก PlayerPrefs
    private string child_id = "1";

    [Header("API Settings")]
    [SerializeField] private string apiURL = "http://localhost/mmff/GetDataPlay.php";
    [SerializeField] private string game_id = "1"; // ระบุ game_id ที่ต้องการ
    [SerializeField] private string score = "0"; // คะแนนที่ได้

    [Header("Chart References")]
    [SerializeField] private BarChart partChart; // Bar Chart สำหรับแสดง part1-5

    [Header("UI Display")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI childIdText;
    [SerializeField] private GameObject loadingPanel;

    [Header("Part Labels")]
    [SerializeField] private string[] partLabels = new string[] 
    { 
        "Part 1", 
        "Part 2", 
        "Part 3", 
        "Part 4", 
        "Part 5" 
    };

    [Header("Colors")]
    [SerializeField] private Color[] barColors = new Color[]
    {
        new Color(0.2f, 0.6f, 1f),    // Part 1 - Blue
        new Color(0.3f, 0.8f, 0.3f),  // Part 2 - Green
        new Color(1f, 0.6f, 0.2f),    // Part 3 - Orange
        new Color(0.9f, 0.3f, 0.3f),  // Part 4 - Red
        new Color(0.7f, 0.3f, 0.9f)   // Part 5 - Purple
    };

    private bool isLoading = false;

    void Start()
    {
        // อ่าน Child_id จาก PlayerPrefs
        LoadChildId();
        
        if (!string.IsNullOrEmpty(child_id))
        {
            StartCoroutine(GetData());
        }
        else
        {
            UpdateStatus("Error: Child_id not found!");
            Debug.LogError("Child_id not set in PlayerPrefs!");
        }
    }

    void LoadChildId()
    {
        if (PlayerPrefs.HasKey(childIdKey))
        {
            child_id = PlayerPrefs.GetInt(childIdKey).ToString();
            Debug.Log($"Loaded Child_id: {child_id}");
            
            if (childIdText != null)
            {
                childIdText.text = $"Child ID: {child_id}";
            }
        }
        else
        {
            Debug.LogWarning("Child_id not found in PlayerPrefs!");
        }
    }

    public IEnumerator GetData()
    {
        if (isLoading) yield break;
        
        isLoading = true;
        ShowLoading(true);
        UpdateStatus("Loading data...");

        string url = apiURL;
        WWWForm form = new WWWForm();
        form.AddField("child_id", child_id);
        form.AddField("game_id", game_id);
        form.AddField("score", score);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                UpdateStatus($"Error: {www.error}");
                Debug.LogError("Request Error: " + www.error);
            }
            else
            {
                string response = www.downloadHandler.text;
                Debug.Log("Response: " + response);

                // แยกข้อมูล part1:part2:part3:part4:part5
                string[] partData = response.Split(':');

                if (partData.Length == 5)
                {
                    // แปลงเป็นตัวเลข
                    float[] partValues = new float[5];
                    bool parseSuccess = true;

                    for (int i = 0; i < 5; i++)
                    {
                        if (float.TryParse(partData[i], out partValues[i]))
                        {
                            Debug.Log($"Part {i + 1}: {partValues[i]}");
                        }
                        else
                        {
                            Debug.LogError($"Failed to parse Part {i + 1}: {partData[i]}");
                            parseSuccess = false;
                            break;
                        }
                    }

                    if (parseSuccess)
                    {
                        UpdateChart(partValues);
                        UpdateStatus("Data loaded successfully!");
                    }
                    else
                    {
                        UpdateStatus("Error: Invalid data format");
                    }
                }
                else
                {
                    UpdateStatus($"Error: Expected 5 values, got {partData.Length}");
                    Debug.LogError($"Invalid response format. Expected 5 parts, got {partData.Length}");
                }
            }
        }

        ShowLoading(false);
        isLoading = false;
    }

    void UpdateChart(float[] partValues)
    {
        if (partChart == null)
        {
            Debug.LogError("BarChart reference not set!");
            return;
        }

        // Clear ข้อมูลเก่า
        partChart.ClearData();

        // ตั้งค่า Title
        var title = partChart.EnsureChartComponent<Title>();
        title.show = true;
        title.text = $"Game Performance - Child {child_id}";
        title.subText = $"Date: {System.DateTime.Now:dd/MM/yyyy}";

        // ตั้งค่า X Axis (Categories)
        var xAxis = partChart.EnsureChartComponent<XAxis>();
        xAxis.show = true;
        xAxis.type = Axis.AxisType.Category;

        // ตั้งค่า Y Axis (Values)
        var yAxis = partChart.EnsureChartComponent<YAxis>();
        yAxis.show = true;
        yAxis.type = Axis.AxisType.Value;
        yAxis.splitNumber = 5;

        // ตั้งค่า Tooltip
        var tooltip = partChart.EnsureChartComponent<Tooltip>();
        tooltip.show = true;

        // ตั้งค่า Legend
        var legend = partChart.EnsureChartComponent<Legend>();
        legend.show = true;

        // เพิ่ม Serie
        var serie = partChart.AddSerie<Bar>("Parts");
        serie.barWidth = 0.6f;
        serie.barGap = 0.3f;

        // เพิ่มข้อมูล
        for (int i = 0; i < partValues.Length; i++)
        {
            // เพิ่ม X-axis label
            partChart.AddXAxisData(partLabels[i]);
            
            // เพิ่มข้อมูล
            var serieData = partChart.AddData(0, partValues[i]);
            
            // ตั้งสีแต่ละแท่ง
            if (serieData != null && i < barColors.Length)
            {
                var itemStyle = serieData.EnsureComponent<ItemStyle>();
                itemStyle.color = barColors[i];
            }
        }

        // Refresh Chart
        partChart.RefreshChart();
        
        Debug.Log("Chart updated successfully!");
    }

    // ===== Helper Methods =====

    void ShowLoading(bool show)
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(show);
        }
    }

    void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"Status: {message}");
    }

    // ===== Public Methods =====

    public void RefreshData()
    {
        if (!isLoading)
        {
            StartCoroutine(GetData());
        }
    }

    public void SetGameId(string newGameId)
    {
        game_id = newGameId;
        Debug.Log($"Game ID set to: {game_id}");
    }

    public void SetScore(string newScore)
    {
        score = newScore;
        Debug.Log($"Score set to: {score}");
    }

    public void SetChildId(int newChildId)
    {
        child_id = newChildId.ToString();
        PlayerPrefs.SetInt(childIdKey, newChildId);
        PlayerPrefs.Save();
        
        if (childIdText != null)
        {
            childIdText.text = $"Child ID: {child_id}";
        }
        
        Debug.Log($"Child ID changed to: {child_id}");
    }

    // ===== Debug Buttons =====
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 200, 250));
        GUILayout.Label("=== Dashboard Debug ===");
        
        GUILayout.Label($"Child ID: {child_id}");
        GUILayout.Label($"Game ID: {game_id}");
        GUILayout.Label($"Score: {score}");
        
        if (GUILayout.Button("Refresh Data"))
        {
            RefreshData();
        }
        
        GUILayout.Space(10);
        GUILayout.Label("--- Test Game IDs ---");
        
        if (GUILayout.Button("Set Game ID = 1"))
        {
            SetGameId("1");
        }
        
        if (GUILayout.Button("Set Game ID = 2"))
        {
            SetGameId("2");
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Set Score = 100"))
        {
            SetScore("100");
        }
        
        if (GUILayout.Button("Reload Data"))
        {
            LoadChildId();
            StartCoroutine(GetData());
        }
        
        GUILayout.EndArea();
    }
}