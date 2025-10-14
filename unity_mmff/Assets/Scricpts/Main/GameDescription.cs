using UnityEngine;
using UnityEngine.Networking;
using TMPro;
public class GameDescription : MonoBehaviour
{
    public TextMeshProUGUI TestMeshPro_Des;
    private int game_id;
    [SerializeField] private GameID gameIdScript;

    public void GetDescription()
    {
        game_id = gameIdScript.GetGameID();
    }

    public void SenttoPHP()
    {
        if (game_id == null)
        {
            Debug.Log("error");
        }
        else
        {
            StartCoroutine(GetDescription());
        }
    }
    
    public IEnumerator GetDescription()
    {
        string url = "http://localhost/mmff_php/GetDescription.php";
        WWWForm form = new WWWForm();
        form.AddField("game_id", game_id);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Request.Success)
            {
                Debug.LogWarning("Error");
            }
            else
            {
                Debug.Log("Reponse" + www.downloadHandler.text);
                string[] posReturn = www.downloadHandler.text;

                Description.text = posReturn[0];
            }
        }
    }
}
