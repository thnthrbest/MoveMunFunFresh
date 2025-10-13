using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class logout : MonoBehaviour
{

    public string sceneName;
    public void Logout()
    {
        PlayerPrefs.SetString("user_id", null);
        SceneManager.LoadScene("LoadingScene");
    }
}
