using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;
using TMPro;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI Gamename, TextDescription;
    public string GameDescription;
    [System.Serializable]
    public class GameNameList
    {
        public string GameName;
    }
    [Header("GameName List")]
    public List<GameNameList> Names = new List<GameNameList>();

    public Image GameImage;

    [System.Serializable]
    public class GameList
    {
        public Sprite GameListSprite;
    }

    [Header("Game List")]
    public List<GameList> GameImages = new List<GameList>();
    public int Currentindex = 0;
    public int game_id = 1;
    void Start()
    {
        game_id = Currentindex + 1;
        if (GameImages.Count > 0)
        {
            UpdateGameImage();
            UpdateText();
            StartCoroutine(SendGameId(game_id));
        }
    }

    public void NextSelectGame()
    {
        if (GameImages.Count == 0 || Names.Count == 0) return;
        Currentindex++;
        game_id = Currentindex + 1;
        if (Currentindex == 4)
        {
            Currentindex = 0;
            game_id = 1;
        }
        UpdateGameImage();
        UpdateText();
        StartCoroutine(SendGameId(game_id));
    }

    public void PreviousSelectGame()
    {
        if (GameImages.Count == 0 || Names.Count == 0) return;
        Currentindex--;
        game_id = Currentindex + 1;
        if (Currentindex == -1)
        {
            Currentindex = GameImages.Count - 1;
            game_id = 4;
        }
        UpdateGameImage();
        UpdateText();
        StartCoroutine(SendGameId(game_id));
    }

    public void OnSelectedGame()
    {
        if (game_id == 1) PlayerPrefs.SetString("game_name", "hand_made");
        else if (game_id == 2) PlayerPrefs.SetString("game_name", "Wall");
        else if (game_id == 3) PlayerPrefs.SetString("game_name", "main_qa");
        else if (game_id == 4) PlayerPrefs.SetString("game_name", "Mosquito");
        SceneManager.LoadScene("ChooseChildScene");
    }

    private void UpdateGameImage()  
    {
        GameImage.sprite = GameImages[Currentindex].GameListSprite;
    }

    private void UpdateText()
    {
        Gamename.text = Names[Currentindex].GameName;
    }

    public IEnumerator SendGameId(int game_id)
    {
        string url = "http://localhost/mmff/GetDescription.php";
        WWWForm form = new WWWForm();
        form.AddField("game_id", game_id);

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
                string[] posReturn = www.downloadHandler.text.Split(':');
                GameDescription = posReturn[0];
                TextDescription.text = GameDescription;
                PlayerPrefs.SetString("game_id", posReturn[1]);
            }
        }
    }
}
