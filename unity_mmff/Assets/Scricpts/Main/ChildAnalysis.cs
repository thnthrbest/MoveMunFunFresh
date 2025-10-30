using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.SceneManagement;

[System.Serializable]
public class ChildAnalysisData
{
    public string child_id;
    public string child_nickname;
}

public class ChildAnalysis : MonoBehaviour
{
    
    [Header("PHP Settings")]
    [Tooltip("URL ของไฟล์ PHP (เช่น http://localhost/childdeepdata.php)")]
    public string phpUrl = "http://localhost/mmff_php/childdeepdata.php";

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
    public Text errorText; // เพิ่มตัวแปรที่ขาดหาย

    private List<ChildAnalysisData> childrenList = new List<ChildAnalysisData>();

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
            
            // ต้องมีอย่างน้อย 6 ฟิลด์ (id, name, nickname, weight, height, age)
            if (childInfo.Length >= 2)
            {
                ChildAnalysisData child = new ChildAnalysisData
                {
                    child_id = childInfo[0],
                    child_nickname = childInfo[1],
                };

                childrenList.Add(child);
                Debug.Log($"เพิ่มเด็ก: {child.child_nickname})");
            }
            else
            {
                Debug.LogWarning($"ข้อมูลไม่ครบ: {childStr} (มี {childInfo.Length} ฟิลด์)");
            }
        }

        Debug.Log($"โหลดข้อมูลเด็ก {childrenList.Count} คน");
    }

    void CreateChildCards()
    {
        // ลบ Card เก่าทั้งหมด
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < childrenList.Count; i++)
        {
            ChildAnalysisData childData = childrenList[i];
            GameObject card = Instantiate(childCardPrefab, gridParent);

            // หา Components
            Image avatarImage = card.transform.Find("Childprofile")?.GetComponent<Image>();
            TextMeshProUGUI nameText = card.transform.Find("ChildNickname")?.GetComponent<TextMeshProUGUI>();
            Button cardButton = card.GetComponent<Button>();

            // ตั้งค่ารูป Avatar
            if (avatarImage != null && avatarSprites.Length > 0)
            {
                int avatarIndex = i % avatarSprites.Length;
                avatarImage.sprite = avatarSprites[avatarIndex];
            }

            // ตั้งค่าชื่อ
            if (nameText != null)
            {
                nameText.text = childData.child_nickname;
            }

            // เพิ่ม Click Event
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

        ChildAnalysisData selectedChild = childrenList[index];
        Debug.Log($"เลือกเด็ก ID: {selectedChild.child_id}, ชื่อ: {selectedChild.child_nickname}");
        PlayerPrefs.SetString("selected_child_id", selectedChild.child_id);
        PlayerPrefs.SetString("selected_child_nickname", selectedChild.child_nickname);
        SceneManager.LoadScene("DashBoard");
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
public class ChildAnalysisCard : MonoBehaviour
{
    public Image avatarImage;
    public Text nameText;
    public Text ageText;
    public Text weightText;
    public Text heightText;

    private ChildAnalysisData childData;

    public void SetData(ChildAnalysisData data, Sprite avatar)
    {
        childData = data;

        if (avatarImage != null)
            avatarImage.sprite = avatar;

        if (nameText != null)
            nameText.text = data.child_nickname;
    }

    public void OnCardClicked()
    {
        Debug.Log($"คลิกที่: {childData.child_nickname} (ID: {childData.child_id})");
    }
}