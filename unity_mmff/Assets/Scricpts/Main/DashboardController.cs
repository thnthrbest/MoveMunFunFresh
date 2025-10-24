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
    private string part1, part2, part3, part4, part5;
    private int score, game_id;
    void Start()
    {
        child_id = PlayerPrefs.GetString("child_id", "Null");
        score = PlayerPrefs.GetInt("Score");
        game_id = PlayerPrefs.GetInt("game_id");
        StartCoroutine(GetDataPlay());
    }

    public IEnumerator GetDataPlay()
    {
        string url = "http://localhost/mmff/GetDataPlay.php";
        WWWForm form = new WWWForm();
        form.AddField("child_id", child_id);
        form.AddField("game_id", game_id);
        form.AddField("score", score);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Error");
            }
            else
            {
                Debug.Log("Reponse: " + www.downloadHandler.text);
                string[] posReturn = www.downloadHandler.text.Split(':');

                part1 = posReturn[0];
                part2 = posReturn[1];
                part3 = posReturn[2];
                part4 = posReturn[3];
                part5 = posReturn[4];
            }
        }
        // LoadData();
    }

    // public void LoadData()
    // {
    //     var serie = partChart.AddSerie<Bar>(partLabels);
    //     serie.stack = " ";
    // }
}