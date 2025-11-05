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

    [Header("Selection Settings")]
    [Tooltip("สีพื้นหลังเมื่อเลือก")]
    public Color selectedColor = new Color(0.3f, 0.8f, 0.3f, 1f); // สีเขียว
    
    [Tooltip("สีพื้นหลังปกติ")]
    public Color normalColor = Color.white;
    
    [Tooltip("จำนวนเด็กสูงสุดที่เลือกได้")]
    public int maxChildren = 5;

    [Header("Loading")]
    public GameObject loadingPanel;
    public Text errorText;
    public Button button;

    [Header("Debug Info")]
    public TextMeshProUGUI selectedCountText; // แสดงจำนวนเด็กที่เลือก (optional)

    private List<ChildDataMulti> childrenList = new List<ChildDataMulti>();
    private Dictionary<string, int> selectedChildOrder = new Dictionary<string, int>(); // child_id -> order number
    private Dictionary<string, GameObject> childCards = new Dictionary<string, GameObject>(); // child_id -> card object
    private int countchild = 1;

    public string game_name;

    void Start()
    {
        userId = PlayerPrefs.GetString("user_id");
        ClearPreviousSelections();
        LoadChildrenData();
        
        if (button != null)
        {
            button.onClick.AddListener(LoadSceneQuiz);
        }
        
        UpdateSelectedCountDisplay();
    }

    void ClearPreviousSelections()
    {
        selectedChildOrder.Clear();
        childCards.Clear();
        countchild = 1;
        
        // ลบ PlayerPrefs ของเด็กทุกคน
        for (int i = 1; i <= maxChildren; i++)
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
        
        childCards.Clear();

        for (int i = 0; i < childrenList.Count; i++)
        {
            ChildDataMulti childData = childrenList[i];
            GameObject card = Instantiate(childCardPrefab, gridParent);

            // เก็บ reference ของ card
            childCards[childData.child_id] = card;

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

            // ตั้งค่าสีพื้นหลังเริ่มต้น
            Image cardBackground = card.GetComponent<Image>();
            if (cardBackground != null)
            {
                cardBackground.color = normalColor;
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
        
        ChildDataMulti selectedChild = childrenList[index];
        string childId = selectedChild.child_id;
        
        // ✅ กรณีที่ 1: เด็กคนนี้ถูกเลือกอยู่แล้ว → ยกเลิกการเลือก
        if (selectedChildOrder.ContainsKey(childId))
        {
            DeselectChild(childId, selectedChild.child_nickname);
            return;
        }
        
        // ✅ กรณีที่ 2: เลือกเด็กใหม่
        // เช็คว่าเลือกครบจำนวนสูงสุดแล้วหรือยัง
        if (selectedChildOrder.Count >= maxChildren)
        {
            Debug.LogWarning($"❌ เลือกเด็กครบ {maxChildren} คนแล้ว");
            ShowError($"คุณเลือกเด็กครบ {maxChildren} คนแล้ว");
            return;
        }
        
        SelectChild(childId, selectedChild.child_nickname);
    }

    /// <summary>
    /// เลือกเด็ก
    /// </summary>
    void SelectChild(string childId, string childNickname)
    {
        // บันทึกลำดับการเลือก
        selectedChildOrder[childId] = countchild;
        
        // บันทึกลง PlayerPrefs
        PlayerPrefs.SetString($"child_id_{countchild}", childId);
        PlayerPrefs.SetString($"child_nickname_{countchild}", childNickname);
        
        Debug.Log($"✅ เลือกเด็กคนที่ {countchild}: {childNickname} (ID: {childId})");
        
        // เปลี่ยนสีพื้นหลัง
        UpdateCardVisual(childId, true);
        
        countchild++;
        UpdateSelectedCountDisplay();
        
        PlayerPrefs.SetInt("CountChild", selectedChildOrder.Count);
        PlayerPrefs.SetInt("score", 0);
        PlayerPrefs.Save();
        
        // แสดงข้อมูลเด็กที่เลือกทั้งหมด
        Debug.Log($"📋 เด็กที่เลือกทั้งหมด ({selectedChildOrder.Count} คน): {string.Join(", ", selectedChildOrder.Keys)}");
    }

    /// <summary>
    /// ยกเลิกการเลือกเด็ก
    /// </summary>
    void DeselectChild(string childId, string childNickname)
    {
        if (!selectedChildOrder.ContainsKey(childId)) return;
        
        int removedOrder = selectedChildOrder[childId];
        selectedChildOrder.Remove(childId);
        
        Debug.Log($"🔄 ยกเลิกการเลือก: {childNickname} (คนที่ {removedOrder})");
        
        // เปลี่ยนสีพื้นหลังกลับเป็นปกติ
        UpdateCardVisual(childId, false);
        
        // จัดเรียงลำดับใหม่
        ReorderSelections();
        
        UpdateSelectedCountDisplay();
        
        PlayerPrefs.SetInt("CountChild", selectedChildOrder.Count);
        PlayerPrefs.Save();
        
        Debug.Log($"📋 เด็กที่เลือกคงเหลือ ({selectedChildOrder.Count} คน)");
    }

    /// <summary>
    /// จัดเรียงลำดับการเลือกใหม่หลังจากยกเลิก
    /// </summary>
    void ReorderSelections()
    {
        // ลบ PlayerPrefs เก่าทั้งหมด
        for (int i = 1; i <= maxChildren; i++)
        {
            PlayerPrefs.DeleteKey($"child_id_{i}");
            PlayerPrefs.DeleteKey($"child_nickname_{i}");
        }
        
        // สร้าง list ของเด็กที่เลือกและเรียงตามลำดับ
        var sortedSelections = new List<KeyValuePair<string, int>>(selectedChildOrder);
        sortedSelections.Sort((a, b) => a.Value.CompareTo(b.Value));
        
        // บันทึกใหม่ตามลำดับ
        Dictionary<string, int> newOrder = new Dictionary<string, int>();
        countchild = 1;
        
        foreach (var pair in sortedSelections)
        {
            string childId = pair.Key;
            
            // หา nickname จาก childrenList
            ChildDataMulti childData = childrenList.Find(c => c.child_id == childId);
            if (childData != null)
            {
                newOrder[childId] = countchild;
                PlayerPrefs.SetString($"child_id_{countchild}", childId);
                PlayerPrefs.SetString($"child_nickname_{countchild}", childData.child_nickname);
                
                Debug.Log($"   จัดเรียงใหม่: คนที่ {countchild} = {childData.child_nickname}");
                countchild++;
            }
        }
        
        selectedChildOrder = newOrder;
        PlayerPrefs.Save();
    }

    /// <summary>
    /// อัพเดทสีพื้นหลังของ Card
    /// </summary>
    void UpdateCardVisual(string childId, bool isSelected)
    {
        if (!childCards.ContainsKey(childId)) return;
        
        GameObject card = childCards[childId];
        Image cardBackground = card.GetComponent<Image>();
        
        if (cardBackground != null)
        {
            cardBackground.color = isSelected ? selectedColor : normalColor;
        }
        
        // ✨ เพิ่ม effect: แสดงเลขลำดับ (optional)
        TextMeshProUGUI orderText = card.transform.Find("OrderText")?.GetComponent<TextMeshProUGUI>();
        if (orderText != null)
        {
            if (isSelected && selectedChildOrder.ContainsKey(childId))
            {
                orderText.text = selectedChildOrder[childId].ToString();
                orderText.gameObject.SetActive(true);
            }
            else
            {
                orderText.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// อัพเดทการแสดงจำนวนเด็กที่เลือก
    /// </summary>
    void UpdateSelectedCountDisplay()
    {
        if (selectedCountText != null)
        {
            selectedCountText.text = $"เลือกแล้ว: {selectedChildOrder.Count}/{maxChildren} คน";
        }
    }

    void ShowError(string message)
    {
        Debug.LogError(message);
        if (errorText != null)
        {
            errorText.text = message;
            errorText.gameObject.SetActive(true);
            
            // ซ่อนข้อความ error หลัง 2 วินาที
            StartCoroutine(HideErrorAfterDelay(2f));
        }
    }

    IEnumerator HideErrorAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (errorText != null)
        {
            errorText.gameObject.SetActive(false);
        }
    }

    public void RefreshData()
    {
        LoadChildrenData();
    }

    public void LoadSceneQuiz()
    {
        // ✅ เช็คว่าเลือกเด็กอย่างน้อย 1 คนหรือยัง
        if (selectedChildOrder.Count == 0)
        {
            ShowError("กรุณาเลือกเด็กอย่างน้อย 1 คน");
            return;
        }
        
        game_name = PlayerPrefs.GetString("game_name");
        Debug.Log($"🎮 โหลดเกม: {game_name} สำหรับเด็ก {selectedChildOrder.Count} คน");
        
        SceneManager.LoadScene("main_qa");
    }

    void OnDestroy()
    {
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
        Debug.Log($"คลิกที่: {childData.child_nickname} (ID: {childData.child_id})");
    }
}