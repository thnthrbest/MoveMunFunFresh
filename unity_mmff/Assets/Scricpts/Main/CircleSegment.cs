using UnityEngine;
using UnityEngine.UI;
public class CircleSegment : MonoBehaviour
{
    [Header("Segment Setting")]
    public Sprite SegmentSprite;
    public int NumberOfSegments = 5;
    public float radius = 150f;

    [Header("Segment Colors")]
    public Color[] SegmentColors = new Color[]
    {
        new Color(0.94f, 0.27f, 0.27f), // Red - Upper Body
        new Color(0.96f, 0.62f, 0.04f), // Orange - Lower Body
        new Color(0.54f, 0.36f, 0.96f), // Purple - Agility
        new Color(0.06f, 0.73f, 0.51f), // Green - Flexibility
        new Color(0.23f, 0.51f, 0.96f)  // Blue - Hand-Eye
    };

    [Header("Labels")]
    public string[] SegmentLabels = new string[]
    {
        "Upper Body",
        "Lower Body",
        "Agility",
        "Flexibity",
        "Hand-Eye"
    };

    private Image[] Segments;

    void Start()
    {
        CreateSegments();
    }

    public void CreateSegments()
    {
        Segments = new Image[NumberOfSegments];
        float angleStep = 360f / NumberOfSegments;

        for (int i = 0; i < NumberOfSegments; i++)
        {
            GameObject SegmentObj = new GameObject($"Segment_{i}_{SegmentLabels[i]}");
            SegmentObj.transform.SetParent(transform);
            SegmentObj.transform.localScale = Vector3.one;
            SegmentObj.transform.localPosition = Vector3.zero;

            Image SegmentImage = SegmentObj.AddComponent<Image>();
            SegmentImage.sprite = SegmentSprite;
            SegmentImage.color = SegmentColors[i];
            SegmentImage.raycastTarget = true;

            RectTransform rectTransform = SegmentObj.GetComponent<RectTransform>();
            rectTransform.pivot = new Vector2(0f, 1f); 
            rectTransform.sizeDelta = new Vector2(radius * 2, radius * 2);
            rectTransform.anchoredPosition = Vector2.zero;

            rectTransform.localRotation = Quaternion.Euler(0, 0, - angleStep * i);

            Segments[i] = SegmentImage;
        }
    }
    
    public void UpdateSegments(int index, float fillAmount)
    {
        if(index >= 0 && index < Segments.Length)
        {
            Color color = SegmentColors[index];
            color.a = 0.3f + (fillAmount * 0.7f);
            Segments[index].color = color;
        }
    }
}