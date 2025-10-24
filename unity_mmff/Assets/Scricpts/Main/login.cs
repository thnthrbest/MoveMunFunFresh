using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;

public class login : MonoBehaviour
{
    public TMP_InputField ID, Password;
    public string sceneName;
    string ID_text, Password_text;
    public void LoginRequest()
    {
        ID_text = ID.text;
        Password_text = Password.text;
        if (string.IsNullOrEmpty(ID_text) || string.IsNullOrEmpty(Password_text))
        {
            Debug.LogWarning("Pls Enter Id and Password");
        }
        else
        {
            StartCoroutine(GetLogin());
        }
    }

    public IEnumerator GetLogin()
    {
        string url = "http://localhost/mmff_php/login.php";
        WWWForm form = new WWWForm();
        form.AddField("id", ID_text);
        form.AddField("password", Password_text);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Error" + www.error);
                PlayerPrefs.SetString("user_id", null);
            }
            else
            {
                Debug.Log("Reponse: " + www.downloadHandler.text);
                string[] posReturn = www.downloadHandler.text.Split(':');

                PlayerPrefs.SetString("user_id", posReturn[0]);
                SceneManager.LoadScene(sceneName);
            }
        }
    }
    
}
