using System;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

[System.Serializable]
public class ChildData
{
    public string child_id;
    public string child_nickname;
}

public class ChildGridManager : MonoBehaviour
{
    [Header("PHP Settings")]
    [Tooltip("URL ของไฟล์ PHP (เช่น http://localhost/choose.php)")]
    public string phpUrl = "http://localhost/mmff_php/choose.php";
    
    [Tooltip("User ID ที่จะดึงข้อมูล")]
    public string userId = "1";

    [Header("UI References")]
    [Tooltip("Prefab ของ Child Card (ต้องมี Image, Text สำหรับชื่อ)")]
    public GameObject childCardPrefab;
    
    [Tooltip("Parent Object ที่จะวาง Card (ควรมี Grid Layout Group)")]
    public Transform gridParent;
    
    [Tooltip("รูปอวาตาร์ทั้งหมด (ต้องเรียงตามลำดับที่ต้องการ)")]
    public Sprite[] avatarSprites;

    [Header("Loading")]
    public GameObject loadingPanel;
    public Text errorText; 

    private List<ChildData> childrenList = new List<ChildData>();

    public string game_name;
    void Start()
    {
        LoadChildrenData();
    }

    public void LoadChildrenData()
    {
        StartCoroutine(FetchChildrenFromServer());
    }

    IEnumerator FetchChildrenFromServer()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        WWWForm form = new WWWForm();
        form.AddField("user_id", userId);

        using (UnityWebRequest www = UnityWebRequest.Post(phpUrl, form))
        {
            yield return www.SendWebRequest();

            if (loadingPanel != null)
                loadingPanel.SetActive(false);

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + www.error);
                ShowError("ไม่สามารถเชื่อมต่อกับเซิร์ฟเวอร์");
            }
            else
            {
                string responseText = www.downloadHandler.text;
                Debug.Log("Response: " + responseText);

                if (responseText == "0")
                {
                    ShowError("No child data");
                }
                else
                {
                    ParseChildrenData(responseText);
                    CreateChildCards();
                }
            }
        }
    }

    void ParseChildrenData(string data)
    {
        childrenList.Clear();

        string[] children = data.Split('#');

        foreach (string childStr in children)
        {
            if (string.IsNullOrEmpty(childStr)) continue;
            string[] childInfo = childStr.Split(':');
            if (childInfo.Length >= 2)
            {
                ChildData child = new ChildData
                {
                    child_id = childInfo[0],
                    child_nickname = childInfo[1]
                };

                childrenList.Add(child);
            }
        }

        Debug.Log($"โหลดข้อมูลเด็ก {childrenList.Count} คน");
    }

    void CreateChildCards()
    {
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < childrenList.Count; i++)
        {
            ChildData childData = childrenList[i];
            GameObject card = Instantiate(childCardPrefab, gridParent);

            Image avatarImage = card.transform.Find("Childprofile")?.GetComponent<Image>();
            TextMeshProUGUI nameText = card.transform.Find("ChildNickname")?.GetComponent<TextMeshProUGUI>();
            Button cardButton = card.GetComponent<Button>();

            if (avatarImage != null && avatarSprites.Length > 0)
            {
                int avatarIndex = i % avatarSprites.Length;
                avatarImage.sprite = avatarSprites[avatarIndex];
            }

            if (nameText != null)
            {
                nameText.text = childData.child_nickname;
            }

            if (cardButton != null)
            {
                int index = i; 
                cardButton.onClick.AddListener(() => OnChildCardClicked(index));
            }
        }
    }

    void OnChildCardClicked(int index)
    {
        if (index < 0 || index >= childrenList.Count) return;

        ChildData selectedChild = childrenList[index];
        Debug.Log($"เลือกเด็ก ID: {selectedChild.child_id}, ชื่อ: {selectedChild.child_nickname}");
        
        PlayerPrefs.SetString("child_id", selectedChild.child_id);
        PlayerPrefs.SetString("child_nickname", selectedChild.child_nickname);
        PlayerPrefs.SetInt("score", 0);
        game_name = PlayerPrefs.GetString("game_name");
        SceneManager.LoadScene(game_name);
        PlayerPrefs.Save();
    }

    void ShowError(string message)
    {
        Debug.LogError(message);
        if (errorText != null)
        {
            errorText.text = message;
            errorText.gameObject.SetActive(true);
        }
    }
    public void RefreshData()
    {
        LoadChildrenData();
    }
}
public class ChildCard : MonoBehaviour
{
    public Image avatarImage;
    public Text nameText;
    
    private ChildData childData;

    public void SetData(ChildData data, Sprite avatar)
    {
        childData = data;
        
        if (avatarImage != null)
            avatarImage.sprite = avatar;
        
        if (nameText != null)
            nameText.text = data.child_nickname;
    }

    public void OnCardClicked()
    {
        // game_name = PlayerPrefs.GetString("game_name");
        Debug.Log($"คลิกที่: {childData.child_nickname} (ID: {childData.child_id})");
        // SceneManager.LoadScene(game_name);
    }
}
