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
    public TextMeshProUGUI Gamename;
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
    private int Currentindex = 0;
    
    void Start()
    {
        if(GameImages.Count > 0)
        {
            UpdateGameImage();
        }
    }

    public void NextSelectGame()
    {
        if (GameImages.Count == 0) return;
        Currentindex++;
        if(Currentindex >= GameImages.Count)
        {
            Currentindex = 0;
        }
        UpdateGameImage();
    }

    public void PreviousSelectGame()
    {
        if (GameImages.Count == 0) return;
        Currentindex--;
        if(Currentindex <= 0)
        {
            Currentindex = GameImages.Count - 1 ;
        }
        UpdateGameImage();
    }
    
    private void UpdateGameImage()
    {
        GameImage.sprite = GameImages[Currentindex].GameListSprite;
    }
}
