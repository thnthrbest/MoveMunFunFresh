using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;

[System.Serializable]
public class ChildScore
{
    public string child_nickname;
    public int part1;
    public int part2;
    public int part3;
    public int part4;
    public int part5;
    public int totalScore;
}
public class MultiEndGame : MonoBehaviour
{
    [Header("Counter References")]
    public NumberCounter part1Counter;
    public NumberCounter part2Counter;
    public NumberCounter part3Counter;
    public NumberCounter part4Counter;
    public NumberCounter part5Counter;

    // ✨ เพิ่มตัวนี้เพื่อให้ DialogueMultiEndGame เข้าถึงได้
    public string[] data;

    private string game_id;
    private int totalChildCount;

    void Start()
    {
        game_id = PlayerPrefs.GetString("game_id");
        totalChildCount = PlayerPrefs.GetInt("CountChild", 1);

        Debug.Log($"Total children: {totalChildCount}");

        StartCoroutine(GetHighestScore());
    }

    IEnumerator GetHighestScore()
    {
        List<ChildScore> allScores = new List<ChildScore>();

        // วนลูปดึงข้อมูลเด็กทุกคน
        for (int i = 1; i <= totalChildCount; i++)
        {
            string child_id = PlayerPrefs.GetString($"child_id_{i}", "");
            string child_nickname = PlayerPrefs.GetString($"child_nickname_{i}", "Unknown");
            int score = PlayerPrefs.GetInt($"score_{i}", 0);

            if (string.IsNullOrEmpty(child_id))
            {
                Debug.LogWarning($"Child {i} has no data");
                continue;
            }

            Debug.Log($"Fetching: {child_nickname} (ID: {child_id})");

            // ส่ง request ไป PHP
            yield return StartCoroutine(FetchScore(child_id, child_nickname, score, allScores));
        }

        // หาคะแนนสูงสุดและแสดงผล
        if (allScores.Count > 0)
        {
            ChildScore highest = GetHighest(allScores);
            ShowCounters(highest);
            
            // ✨ เก็บข้อมูลของคนที่คะแนนสูงสุดไว้ใน data array
            StoreHighestScoreData(highest);
        }
        else
        {
            Debug.LogError("No scores found!");
        }
    }

    IEnumerator FetchScore(string child_id, string child_nickname, int score, List<ChildScore> scoreList)
    {
        string url = "http://localhost/mmff_php/GetDataPlay.php";
        WWWForm form = new WWWForm();
        form.AddField("child_id", child_id);
        form.AddField("game_id", game_id);
        form.AddField("score", score);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            www.timeout = 10;
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {www.error}");
                yield break;
            }

            string response = www.downloadHandler.text.Trim();
            Debug.Log($"{child_nickname} response: {response}");

            string[] responseData = response.Split(':');
            if (responseData.Length != 5)
            {
                Debug.LogError($"Invalid data for {child_nickname}");
                yield break;
            }

            // แปลงเป็นตัวเลข
            int[] parts = new int[5];
            int total = 0;

            for (int i = 0; i < 5; i++)
            {
                if (int.TryParse(responseData[i], out int value))
                {
                    parts[i] = value;
                    total += value;
                }
            }

            // เก็บข้อมูล
            ChildScore childScore = new ChildScore
            {
                child_nickname = child_nickname,
                part1 = parts[0],
                part2 = parts[1],
                part3 = parts[2],
                part4 = parts[3],
                part5 = parts[4],
                totalScore = total
            };

            scoreList.Add(childScore);
            Debug.Log($"✓ {child_nickname}: {total} points ({parts[0]}, {parts[1]}, {parts[2]}, {parts[3]}, {parts[4]})");
        }
    }

    ChildScore GetHighest(List<ChildScore> scores)
    {
        ChildScore highest = scores[0];

        foreach (ChildScore score in scores)
        {
            if (score.totalScore > highest.totalScore)
            {
                highest = score;
            }
        }

        Debug.Log($"🏆 Highest: {highest.child_nickname} with {highest.totalScore} points");
        return highest;
    }

    void ShowCounters(ChildScore highest)
    {
        NumberCounter[] counters = {
            part1Counter,
            part2Counter,
            part3Counter,
            part4Counter,
            part5Counter
        };

        int[] parts = {
            highest.part1,
            highest.part2,
            highest.part3,
            highest.part4,
            highest.part5
        };

        for (int i = 0; i < counters.Length; i++)
        {
            if (counters[i] != null)
            {
                int value = Mathf.Min(parts[i], 10); // จำกัดไม่เกิน 10
                counters[i].SetTargetValue(value);
                Debug.Log($"Counter {i + 1}: {value}");
            }
        }

        Debug.Log("✓ Counters updated!");
    }

    // ✨ ฟังก์ชันใหม่: เก็บข้อมูลของคนที่คะแนนสูงสุดไว้ใน data array
    void StoreHighestScoreData(ChildScore highest)
    {
        data = new string[5];
        data[0] = highest.part1.ToString();
        data[1] = highest.part2.ToString();
        data[2] = highest.part3.ToString();
        data[3] = highest.part4.ToString();
        data[4] = highest.part5.ToString();

        Debug.Log($"✓ Data stored: {string.Join(", ", data)}");
    }

    public void ChooseAnotherGame()
    {
        // ลบข้อมูล
        PlayerPrefs.DeleteKey("score");
        PlayerPrefs.DeleteKey("game_id");
        PlayerPrefs.DeleteKey("CountChild");

        for (int i = 1; i <= 5; i++)
        {
            PlayerPrefs.DeleteKey($"child_id_{i}");
            PlayerPrefs.DeleteKey($"child_nickname_{i}");
            PlayerPrefs.DeleteKey($"score_{i}");
        }

        PlayerPrefs.Save();
        SceneManager.LoadScene("ChooseGame");
    }
}
