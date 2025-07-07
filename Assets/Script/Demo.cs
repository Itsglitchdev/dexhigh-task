using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Demo : MonoBehaviour
{
    [Header("Circle Settings")]
    [SerializeField] private float radius = 100f;
    [Range(0, 360)][SerializeField] private float totalAngle = 230f;
    [SerializeField] private float angleOffset = 89.5f;

    [Header("Settings")]
    [SerializeField] private bool startOnAwake = true;
    [SerializeField] private bool liveUpdate = false;
    [SerializeField] private float rotationDuration = 1.5f;

    private List<RectTransform> currentButtonOrder = new List<RectTransform>();
    private bool isRotating = false;

    void Start()
    {
        if (startOnAwake)
        {
            CacheButtons();
            ArrangeButtons();
            AddListeners();
        }
    }

    void Update()
    {
        if (liveUpdate)
        {
            ArrangeButtons();
        }
    }

    private void CacheButtons()
    {
        currentButtonOrder.Clear();
        foreach (Transform child in transform)
        {
            currentButtonOrder.Add(child.GetComponent<RectTransform>());
        }
    }

    private void AddListeners()
    {
        // Clear existing listeners first
        for (int i = 0; i < currentButtonOrder.Count; i++)
        {
            currentButtonOrder[i].GetComponent<Button>().onClick.RemoveAllListeners();
        }

        // Add new listeners with current indices
        for (int i = 0; i < currentButtonOrder.Count; i++)
        {
            int index = i;
            currentButtonOrder[i].GetComponent<Button>().onClick.AddListener(() => OnButtonClicked(index));
        }
    }

    private void OnButtonClicked(int clickedIndex)
    {
        if (isRotating) return;

        int centerIndex = currentButtonOrder.Count / 2;
        Debug.Log($"Center Index: {centerIndex}, Clicked Index: {clickedIndex}");

        int steps;
        if (clickedIndex < centerIndex)
        {
            // Button is on the left side - rotate anti-clockwise 
            steps = centerIndex - clickedIndex;
        }
        else if (clickedIndex > centerIndex)
        {
            // Button is on the right side - rotate clockwise 
            steps = -(clickedIndex - centerIndex);
        }
        else
        {
            // Button is already at center - no rotation needed
            return;
        }

        Debug.Log($"Rotating {steps} steps ({(steps > 0 ? "anti-clockwise" : "clockwise")})");
        StartCoroutine(RotateButtons(steps));
    }

    private IEnumerator RotateButtons(int steps)
    {
        isRotating = true;

        int count = currentButtonOrder.Count;
        float angleStep = totalAngle / count;

        // Calculate current angles from layout index, not from position
        List<float> startAngles = new List<float>();
        for (int i = 0; i < count; i++)
        {
            Vector2 pos = currentButtonOrder[i].anchoredPosition.normalized;
            float angle = Mathf.Atan2(pos.y, pos.x) * Mathf.Rad2Deg;
            startAngles.Add((angle + 360f) % 360f);
        }

        List<float> targetAngles = new List<float>();
        for (int i = 0; i < count; i++)
        {
            int targetIndex = (i + steps + count) % count;
            float layoutAngle = angleOffset + angleStep * targetIndex;
            layoutAngle %= 360f;

            float start = startAngles[i];
            float delta = Mathf.DeltaAngle(start, layoutAngle);

            // This ensures full arc travel if needed (force direction based on steps)
            if (steps > 0 && delta < 0)
                delta += 360f;
            else if (steps < 0 && delta > 0)
                delta -= 360f;

            float target = start + delta;
            targetAngles.Add(target);
        }

        float elapsed = 0f;
        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / rotationDuration);

            for (int i = 0; i < count; i++)
            {
                float angle = Mathf.Lerp(startAngles[i], targetAngles[i], t);
                float rad = angle * Mathf.Deg2Rad;
                Vector2 pos = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
                currentButtonOrder[i].anchoredPosition = pos;
            }

            yield return null;
        }

        // Reorder list logically
        List<RectTransform> newOrder = new List<RectTransform>(currentButtonOrder);
        for (int i = 0; i < Mathf.Abs(steps); i++)
        {
            if (steps > 0)
            {
                RectTransform last = newOrder[^1];
                newOrder.RemoveAt(newOrder.Count - 1);
                newOrder.Insert(0, last);
            }
            else
            {
                RectTransform first = newOrder[0];
                newOrder.RemoveAt(0);
                newOrder.Add(first);
            }
        }

        currentButtonOrder = newOrder;

        ArrangeButtons();
        AddListeners();
        isRotating = false;

        // // Debug.Log("New order:");
        //     // for (int i = 0; i < currentButtonOrder.Count; i++)
        //     // {
        //     //     Debug.Log($"Index {i}: {currentButtonOrder[i].name}");
        //     // }
        // }
    }

    private void ArrangeButtons()
    {
        int count = currentButtonOrder.Count;
        float angleStep = totalAngle / count;

        for (int i = 0; i < count; i++)
        {
            float angle = angleOffset + angleStep * i;
            float rad = angle * Mathf.Deg2Rad;
            Vector2 pos = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;

            currentButtonOrder[i].anchoredPosition = pos;

            float normalizedAngle = (angle + 360f) % 360f;
            Debug.Log($"[AbilityButton {i}] Angle: {normalizedAngle:F2}°, Position: {pos}");
        }
    }
}