using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
public class Water : MonoBehaviour
{
    [Header("Sprite References")]
    public Sprite segmentSprite; // Sprite รูปสามเหลี่ยม 72 องศา
    public Sprite centerOverlaySprite; // วงกลมโปร่งใสสำหรับ Layer 3
    
    [Header("Settings")]
    public float radius = 200f;
    public float maxScore = 10f;
    public int numberOfSegments = 5;
    
    [Header("Segment Colors")]
    public Color[] segmentColors = new Color[]
    {
        new Color(0.94f, 0.27f, 0.27f), // Red - Upper Body (part1)
        new Color(0.96f, 0.62f, 0.04f), // Orange - Lower Body (part2)
        new Color(0.54f, 0.36f, 0.96f), // Purple - Agility (part3)
        new Color(0.06f, 0.73f, 0.51f), // Green - Flexibility (part4)
        new Color(0.23f, 0.51f, 0.96f)  // Blue - Hand-Eye (part5)
    };
    
    [Header("Layer References")]
    private GameObject layer1_Background;
    private GameObject layer2_WaterFill;
    private GameObject layer3_Overlay;
    
    [Header("Water Segments")]
    private List<WaterSegmentController> waterSegments = new List<WaterSegmentController>();
    
    void Start()
    {
        CreateCircleDisplay();
    }
    
    void CreateCircleDisplay()
    {
        // ชั้นที่ 1: Background Segments (โปร่งใส)
        layer1_Background = new GameObject("Layer1_BackgroundSegments");
        layer1_Background.transform.SetParent(transform);
        layer1_Background.transform.localPosition = Vector3.zero;
        layer1_Background.transform.localScale = Vector3.one;
        CreateBackgroundSegments();
        
        // ชั้นที่ 2: Water Fill
        layer2_WaterFill = new GameObject("Layer2_WaterFill");
        layer2_WaterFill.transform.SetParent(transform);
        layer2_WaterFill.transform.localPosition = Vector3.zero;
        layer2_WaterFill.transform.localScale = Vector3.one;
        CreateWaterSegments();
        
        // ชั้นที่ 3: Overlay
        layer3_Overlay = new GameObject("Layer3_Overlay");
        layer3_Overlay.transform.SetParent(transform);
        layer3_Overlay.transform.localPosition = Vector3.zero;
        layer3_Overlay.transform.localScale = Vector3.one;
        CreateOverlay();
    }
    
    void CreateBackgroundSegments()
    {
        float angleStep = 360f / numberOfSegments;
        
        for (int i = 0; i < numberOfSegments; i++)
        {
            GameObject segmentObj = new GameObject($"Segment_{i}");
            segmentObj.transform.SetParent(layer1_Background.transform);
            segmentObj.transform.localScale = Vector3.one;
            segmentObj.transform.localPosition = Vector3.zero;
            
            Image segmentImage = segmentObj.AddComponent<Image>();
            segmentImage.sprite = segmentSprite;
            
            // สีโปร่งใส 30%
            Color bgColor = segmentColors[i];
            bgColor.a = 0.3f;
            segmentImage.color = bgColor;
            
            RectTransform rectTransform = segmentObj.GetComponent<RectTransform>();
            rectTransform.pivot = new Vector2(0.5f, 0f); // ปรับตาม Sprite ของคุณ
            rectTransform.sizeDelta = new Vector2(radius * 2, radius * 2);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localRotation = Quaternion.Euler(0, 0, -angleStep * i);
        }
    }
    
    void CreateWaterSegments()
    {
        float angleStep = 360f / numberOfSegments;
        
        for (int i = 0; i < numberOfSegments; i++)
        {
            // สร้าง Empty Object พร้อม Mask
            GameObject waterSegmentObj = new GameObject($"WaterSegment_{i}");
            waterSegmentObj.transform.SetParent(layer2_WaterFill.transform);
            waterSegmentObj.transform.localScale = Vector3.one;
            waterSegmentObj.transform.localPosition = Vector3.zero;
            
            // เพิ่ม Image Component สำหรับ Mask
            Image maskImage = waterSegmentObj.AddComponent<Image>();
            maskImage.sprite = segmentSprite;
            maskImage.color = new Color(1, 1, 1, 0); // โปร่งใส แต่ทำหน้าที่เป็น mask
            
            // เพิ่ม Mask Component
            Mask mask = waterSegmentObj.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            
            RectTransform maskRect = waterSegmentObj.GetComponent<RectTransform>();
            maskRect.pivot = new Vector2(0.5f, 0f); // ปรับตาม Sprite
            maskRect.sizeDelta = new Vector2(radius * 2, radius * 2);
            maskRect.anchoredPosition = Vector2.zero;
            maskRect.localRotation = Quaternion.Euler(0, 0, -angleStep * i);
            
            // สร้าง Water Object (ลูกของ Mask)
            GameObject waterObj = new GameObject("Water");
            waterObj.transform.SetParent(waterSegmentObj.transform);
            waterObj.transform.localScale = Vector3.one;
            waterObj.transform.localPosition = Vector3.zero;
            waterObj.transform.localRotation = Quaternion.identity;
            
            Image waterImage = waterObj.AddComponent<Image>();
            waterImage.sprite = segmentSprite;
            waterImage.color = segmentColors[i];
            waterImage.type = Image.Type.Filled;
            waterImage.fillMethod = Image.FillMethod.Vertical;
            waterImage.fillOrigin = (int)Image.OriginVertical.Bottom;
            waterImage.fillAmount = 0f; // เริ่มที่ 0
            
            RectTransform waterRect = waterObj.GetComponent<RectTransform>();
            waterRect.sizeDelta = new Vector2(radius * 2, radius * 2);
            waterRect.anchoredPosition = Vector2.zero;
            
            // เพิ่ม Water Effect Component
            WaterWaveEffect waveEffect = waterObj.AddComponent<WaterWaveEffect>();
            waveEffect.Initialize(segmentColors[i]);
            
            // เพิ่ม Controller
            WaterSegmentController controller = waterSegmentObj.AddComponent<WaterSegmentController>();
            controller.waterImage = waterImage;
            controller.waveEffect = waveEffect;
            controller.maxScore = maxScore;
            
            waterSegments.Add(controller);
        }
    }
    
    void CreateOverlay()
    {
        GameObject overlayObj = new GameObject("CenterOverlay");
        overlayObj.transform.SetParent(layer3_Overlay.transform);
        overlayObj.transform.localScale = Vector3.one;
        overlayObj.transform.localPosition = Vector3.zero;
        
        Image overlayImage = overlayObj.AddComponent<Image>();
        
        if (centerOverlaySprite != null)
        {
            overlayImage.sprite = centerOverlaySprite;
        }
        
        // วงกลมโปร่งใสขาว
        overlayImage.color = new Color(1f, 1f, 1f, 0.1f);
        
        RectTransform overlayRect = overlayObj.GetComponent<RectTransform>();
        overlayRect.sizeDelta = new Vector2(radius * 2.2f, radius * 2.2f);
        overlayRect.anchoredPosition = Vector2.zero;
    }
    
    // ฟังก์ชันสำหรับอัพเดทคะแนนจาก EndGame.cs
    public void UpdateScores(float[] scores)
    {
        for (int i = 0; i < scores.Length && i < waterSegments.Count; i++)
        {
            waterSegments[i].SetScore(scores[i]);
        }
    }
    
    // ฟังก์ชันสำหรับอัพเดทคะแนนจากข้อความที่แยกมาจาก PHP
    public void UpdateScoresFromText(string part1, string part2, string part3, string part4, string part5)
    {
        float[] scores = new float[5];
        
        float.TryParse(part1, out scores[0]);
        float.TryParse(part2, out scores[1]);
        float.TryParse(part3, out scores[2]);
        float.TryParse(part4, out scores[3]);
        float.TryParse(part5, out scores[4]);
        
        UpdateScores(scores);
    }
}

// Component สำหรับควบคุมแต่ละ Water Segment
public class WaterSegmentController : MonoBehaviour
{
    public Image waterImage;
    public WaterWaveEffect waveEffect;
    public float maxScore = 10f;
    
    private float currentScore = 0f;
    private float targetFillAmount = 0f;
    private float fillSpeed = 0.5f;
    
    void Update()
    {
        // Smooth fill animation
        if (waterImage.fillAmount != targetFillAmount)
        {
            waterImage.fillAmount = Mathf.Lerp(waterImage.fillAmount, targetFillAmount, Time.deltaTime * fillSpeed);
        }
    }
    
    public void SetScore(float score)
    {
        currentScore = Mathf.Clamp(score, 0f, maxScore);
        targetFillAmount = currentScore / maxScore;
        
        if (waveEffect != null)
        {
            waveEffect.SetIntensity(targetFillAmount);
        }
    }
}

// Component สำหรับเอฟเฟกต์คลื่นน้ำ
public class WaterWaveEffect : MonoBehaviour
{
    private Material waterMaterial;
    private float waveSpeed = 1f;
    private float waveAmount = 0.01f;
    private float intensity = 0f;
    
    public void Initialize(Color waterColor)
    {
        Image image = GetComponent<Image>();
        if (image != null)
        {
            // สร้าง Material ใหม่
            waterMaterial = new Material(Shader.Find("UI/Default"));
            waterMaterial.color = waterColor;
            image.material = waterMaterial;
        }
    }
    
    public void SetIntensity(float value)
    {
        intensity = value;
    }
    
    void Update()
    {
        if (waterMaterial != null && intensity > 0)
        {
            // สร้างเอฟเฟกต์คลื่นด้วย offset
            float wave = Mathf.Sin(Time.time * waveSpeed) * waveAmount * intensity;
            transform.localPosition = new Vector3(0, wave * 10f, 0);
        }
    }
}
