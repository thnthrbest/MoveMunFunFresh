using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.Networking;
public class GetDescription : MonoBehaviour
{
    public GameManager gameManager;
    public TextMeshProUGUI TextDescription;
    public int Game_id;
    void Start()
    {
        Game_id = gameManager.ReturnId();
        StartCoroutine(SendGameId());
    }

    public void Click()
    {
        StartCoroutine(SendGameId());
    }

    public IEnumerator SendGameId()
    {
        string url = "http://localhost/mmff/GetDescription.php";
        WWWForm form = new WWWForm();
        form.AddField("game_id", Game_id);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Error");
            }
            else
            {
                Debug.Log("Reponse: " + www.downloadHandler.text);
                TextDescription.text = www.downloadHandler.text;
            }
        }
    }
}
