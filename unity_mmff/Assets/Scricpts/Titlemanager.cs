using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
public class Titlemanager : MonoBehaviour
{
    
    void Start()
    {
        Invoke("update", 3f);
    }

    void Update()
    {
        if (PlayerPrefs.HasKey("user_id"))
        {
            string user_id = PlayerPrefs.GetString("user_id");
            if (string.IsNullOrEmpty(user_id))
            {
                SceneManager.LoadScene("LoginScene");
            }
            else
            {
                SceneManager.LoadScene("AccountScene");
            }
        }
        else
        {
            SceneManager.LoadScene("LoginScene");
        }
    }
}
