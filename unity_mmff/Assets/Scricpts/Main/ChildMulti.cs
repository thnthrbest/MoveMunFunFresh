using System;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

[System.Serializable]
public class ChildDataMulti
{
    public string child_id;
    public string child_nickname;
}

public class ChildMulti : MonoBehaviour
{
    [Header("PHP Settings")]
    [Tooltip("URL ของไฟล์ PHP (เช่น http://localhost/choose.php)")]
    public string phpUrl = "http://localhost/mmff_php/choose.php";

    [Tooltip("User ID ที่จะดึงข้อมูล")]
    public string userId;

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

    private List<ChildDataMulti> childrenList = new List<ChildDataMulti>();
    public int countchild = 1;
    private HashSet<string> selectedChildIds = new HashSet<string>();

    public Button button;

    public string game_name;
    void Start()
    {
        userId = PlayerPrefs.GetString("user_id");

        // ✅ รีเซ็ตข้อมูลเด็กที่เลือกเมื่อเริ่มใหม่
        ClearPreviousSelections();

        LoadChildrenData();
        if (button != null)
        {
            button.onClick.AddListener(LoadSceneQuiz);
        }
    }

    void ClearPreviousSelections()
    {
        selectedChildIds.Clear();
        countchild = 1;
        
        // ลบ PlayerPrefs ของเด็กทุกคน
        for (int i = 1; i <= 5; i++)
        {
            PlayerPrefs.DeleteKey($"child_id_{i}");
            PlayerPrefs.DeleteKey($"child_nickname_{i}");
        }
        PlayerPrefs.SetInt("CountChild", 0);
        PlayerPrefs.Save();
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
                ChildDataMulti child = new ChildDataMulti
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
            ChildDataMulti childData = childrenList[i];
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
        ChildDataMulti selectedChild = childrenList[index];
        
        // ✅ เช็คว่าเด็กคนนี้ถูกเลือกไปแล้วหรือยัง
        if (selectedChildIds.Contains(selectedChild.child_id))
        {
            Debug.LogWarning($"❌ เด็กคนนี้ถูกเลือกแล้ว: {selectedChild.child_nickname}");
            ShowError($"คุณเลือก {selectedChild.child_nickname} ไปแล้ว กรุณาเลือกเด็กคนอื่น");
            return;
        }

        // ✅ เช็คว่าเลือกครบ 5 คนแล้วหรือยัง
        if (countchild > 5)
        {
            Debug.LogWarning("❌ เลือกเด็กครบ 5 คนแล้ว");
            ShowError("คุณเลือกเด็กครบ 5 คนแล้ว");
            return;
        }

        // ✅ บันทึกข้อมูล
        selectedChildIds.Add(selectedChild.child_id);
        PlayerPrefs.SetString($"child_id_{countchild}", selectedChild.child_id);
        PlayerPrefs.SetString($"child_nickname_{countchild}", selectedChild.child_nickname);
        
        Debug.Log($"✅ เลือกเด็กคนที่ {countchild}: {selectedChild.child_nickname} (ID: {selectedChild.child_id})");
        
        countchild++;
        PlayerPrefs.SetInt("CountChild", countchild - 1); // บันทึกจำนวนเด็กที่เลือกจริง
        PlayerPrefs.SetInt("score", 0);
        PlayerPrefs.Save();
        
        game_name = PlayerPrefs.GetString("game_name");
        
        // แสดงข้อมูลเด็กที่เลือกทั้งหมด
        Debug.Log($"📋 เด็กที่เลือกทั้งหมด ({selectedChildIds.Count} คน): {string.Join(", ", selectedChildIds)}");
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
    public void LoadSceneQuiz()
    {
        // ✅ เช็คว่าเลือกเด็กอย่างน้อย 1 คนหรือยัง
        if (countchild <= 1)
        {
            ShowError("กรุณาเลือกเด็กอย่างน้อย 1 คน");
            return;
        }
        
        SceneManager.LoadScene("main_qa");
    }
    void OnDestroy()
    {
        // ลบ listener เมื่อ object ถูกทำลาย (ป้องกัน memory leak)
        if (button != null)
        {
            button.onClick.RemoveListener(LoadSceneQuiz);
        }
    }
}
public class ChildMultiCard : MonoBehaviour
{
    public Image avatarImage;
    public Text nameText;
    
    private ChildDataMulti childData;

    public void SetData(ChildDataMulti data, Sprite avatar)
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
