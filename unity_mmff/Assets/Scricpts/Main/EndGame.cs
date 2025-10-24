using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;
public class EndGame : MonoBehaviour
{
    private int i = 1;
    public TextMeshProUGUI part1, part2, part3, part4, part5;
    public string child_id, game_id, score;
    public void PHP()
    {
        if(i > 0)
        {
            StartCoroutine(GetPoint());
        }
    }
    
    public IEnumerator GetPoint()
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

                part1.text = posReturn[0];
                part2.text = posReturn[1];
                part3.text = posReturn[2];
                part4.text = posReturn[3];
                part5.text = posReturn[4];
            }
        }
    }
}
