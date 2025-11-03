using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;
public class EndGame : MonoBehaviour
{
    public NumberCounter part1Counter;
    public NumberCounter part2Counter;
    public NumberCounter part3Counter;
    public NumberCounter part4Counter;
    public NumberCounter part5Counter;
    
    private string child_id;
    private string game_id;
    private int score;

    public string[] data;
    void Start()
    {
        child_id = PlayerPrefs.GetString("child_id");
        game_id = PlayerPrefs.GetString("game_id");
        score = PlayerPrefs.GetInt("score");
        
        StartCoroutine(GetPoint());
    }

    public IEnumerator GetPoint()
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

            if (string.IsNullOrEmpty(www.downloadHandler.text))
            {
                Debug.LogError("No data received");
                yield break;
            }

            Debug.Log("Response: " + www.downloadHandler.text);

            data = www.downloadHandler.text.Split(':');
            if (data.Length != 5)
            {
                Debug.LogError($"Invalid data. Expected 5 values, got {data.Length}");
                yield break;
            }

            // ให้ NumberCounter นับอัตโนมัติ
            NumberCounter[] counters = { part1Counter, part2Counter, part3Counter, part4Counter, part5Counter };

            for (int i = 0; i < counters.Length; i++)
            {
                if (int.TryParse(data[i], out int value))
                {
                    value = Mathf.Min(value, 10);
                    if (counters[i] != null)
                    {
                        counters[i].SetTargetValue(value);
                    }
                }
            }

        }

        
    }
    
    public void ChooseAnotherGame()
    {
        PlayerPrefs.SetInt("score", 0);
        PlayerPrefs.SetString("game_id", null);
        PlayerPrefs.SetString("child_id", null);

        SceneManager.LoadScene("ChooseGame");
    }
}
