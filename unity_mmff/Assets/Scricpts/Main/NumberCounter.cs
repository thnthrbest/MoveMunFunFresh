using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class NumberCounter : MonoBehaviour
{
    public TextMeshProUGUI Text;
    public float Duration = 1f;
    public string NumberFormat = "N0";
    public AnimationCurve Easing = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine countingCoroutine;
    private float currentValue = 0f;

    private void Awake()
    {
        Text = GetComponent<TextMeshProUGUI>();
    }

    public void SetTargetValue(float newValue)
    {
        if (countingCoroutine != null)
            StopCoroutine(countingCoroutine);

        countingCoroutine = StartCoroutine(CountTo(newValue));
    }

    private IEnumerator CountTo(float target)
    {
        float start = currentValue;
        float time = 0f;

        while (time < Duration)
        {
            time += Time.deltaTime;
            float t = time / Duration;
            float eased = Easing.Evaluate(t);

            currentValue = Mathf.Lerp(start, target, eased);
            Text.text = currentValue.ToString(NumberFormat);

            yield return null;
        }

        currentValue = target;
        Text.text = target.ToString(NumberFormat);
    }
}
