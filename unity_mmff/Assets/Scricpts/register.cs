using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;

public class register : MonoBehaviour
{
    public TMP_InputField Id, Classroom, Username, Password;
    string ID_text, Classroom_text, Username_text, Password_text;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // public void LoadScene(string RegisterScene)
    // {
    //     SceneManager.LoadScene("RegisterScene");
    // }

    public void RegisterRequest()
    {
        ID_text = Id.text;
        Classroom_text = Classroom.text;
        Username_text = Username.text;
        Password_text = Password.text;

        if (string.IsNullOrEmpty(ID_text) || string.IsNullOrEmpty(Classroom_text) || string.IsNullOrEmpty(Username_text) || string.IsNullOrEmpty(Password_text))
        {
            Debug.Log("Put the text in the box");
        }
        else
        {
            StartCoroutine(UpdateData());
        }
    }

    public IEnumerator UpdateData()
    {
        string url = "http://localhost//mmff/register.php";
        WWWForm form = new WWWForm();
        form.AddField("id", ID_text);
        form.AddField("classroom", Classroom_text);
        form.AddField("username", Username_text);
        form.AddField("password", Password_text);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("0");
            }
            else
            {
                Debug.Log("1");
            }
        }
    }
}
