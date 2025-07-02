using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityButtonsLayout : MonoBehaviour
{
    [Header("Layout Settings")]
    [SerializeField] private RectTransform centerButton;
    [SerializeField] private RectTransform[] buttons;
    [SerializeField] private float radius = 70f;
    [SerializeField] private float angleOffset = 16.5f;
    [SerializeField] private float spacing = 1f;
    [SerializeField] private float layoutDuration = 0.5f;
    [SerializeField] private float rotationDuration = 0.3f;

    [Header("Text Panel")]
    [SerializeField] private GameObject panelText;
    [SerializeField] private TextMeshProUGUI[] abilityTexts;

    [Header("Editor Testing")]
    [SerializeField] private bool livePreviewEnabled = false;
    [SerializeField] private bool forceUpdateLayout = false;

    private Coroutine layoutRoutine;
    private Coroutine rotationRoutine;
    private int currentCenterIndex = 2;

    private readonly Vector2 collapsedSize = new Vector2(75f, 75f);
    private readonly Vector2[] expandedSizes = new Vector2[]
    {
        new Vector2(65f, 65f), // First
        new Vector2(70f, 70f), // Second
        new Vector2(75f, 75f), // Middle (Center)
        new Vector2(70f, 70f), // Fourth
        new Vector2(65f, 65f), // Last
    };
    private bool hasSavedOrder = false;

    // Logical order of abilities
    private readonly string[] abilityNames = { "Wind", "Light", "Water", "Fire", "Dark" };
    private int[] currentButtonOrder = new int[] { 0, 1, 2, 3, 4 };
    private RectTransform[] originalButtons;

    void Start()
    {
        // ✅ Store the original button array
        originalButtons = new RectTransform[buttons.Length];
        for (int i = 0; i < buttons.Length; i++)
        {
            originalButtons[i] = buttons[i];
        }

        ApplyLayout();
        SetupButtonClickEvents();
        UpdateTextDisplay();
    }


    void Update()
    {
#if UNITY_EDITOR
        if (livePreviewEnabled && forceUpdateLayout)
        {
            ApplyLayout();
            forceUpdateLayout = false;
        }
#endif
    }

    private void SetupButtonClickEvents()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int buttonIndex = i; // Capture for closure
            Button buttonComponent = buttons[i].GetComponent<Button>();
            if (buttonComponent != null)
            {
                buttonComponent.onClick.AddListener(() => OnAbilityButtonClick(buttonIndex));
            }
        }
    }

    public void OnAbilityButtonClick(int clickedVisualIndex)
    {
        if (rotationRoutine != null)
            StopCoroutine(rotationRoutine);

        if (clickedVisualIndex == 2)
            return; // Already center

        // Rotate based on visual index difference
        int steps = (2 - clickedVisualIndex + 5) % 5;
        if (steps > 2) steps -= 5;

        rotationRoutine = StartCoroutine(RotateToCenter(steps));
    }




    private IEnumerator RotateToCenter(int steps)
    {
        Vector2[] startPositions = new Vector2[buttons.Length];
        Vector2[] startSizes = new Vector2[buttons.Length];

        for (int i = 0; i < buttons.Length; i++)
        {
            startPositions[i] = buttons[i].anchoredPosition;
            startSizes[i] = buttons[i].sizeDelta;
        }

        Vector2[] targetPositions = new Vector2[buttons.Length];
        Vector2[] targetSizes = new Vector2[buttons.Length];

        CalculateRotatedLayout(steps, targetPositions, targetSizes);

        float elapsed = 0f;

        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / rotationDuration;

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].anchoredPosition = Vector2.Lerp(startPositions[i], targetPositions[i], t);
                buttons[i].sizeDelta = Vector2.Lerp(startSizes[i], targetSizes[i], t);
            }

            yield return null;
        }

        // Snap to final layout
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].anchoredPosition = targetPositions[i];
            buttons[i].sizeDelta = targetSizes[i];
        }

        UpdateButtonOrderAfterRotation(steps);

        RectTransform[] newButtonArray = new RectTransform[5];
        for (int i = 0; i < 5; i++)
        {
            newButtonArray[i] = originalButtons[currentButtonOrder[i]];
        }
        buttons = newButtonArray;

        currentCenterIndex = 2;
        UpdateTextDisplay();
    }

    private void UpdateButtonOrderAfterRotation(int steps)
    {
        int[] newOrder = new int[5];
        for (int i = 0; i < 5; i++)
        {
            newOrder[i] = currentButtonOrder[(i - steps + 5) % 5];
        }
        currentButtonOrder = newOrder;
    }


    private void CalculateRotatedLayout(int steps, Vector2[] targetPositions, Vector2[] targetSizes)
    {
        // Create a mapping of current logical positions to new positions
        int[] newLogicalOrder = new int[5];

        for (int i = 0; i < 5; i++)
        {
            newLogicalOrder[i] = (i - steps + 5) % 5;
        }

        // Calculate positions for the new layout
        int count = buttons.Length;
        float angleRange = 180f * spacing;
        float angleStep = (count > 1) ? angleRange / count : 0f;

        for (int i = 0; i < count; i++)
        {
            float angle = (angleOffset + i * angleStep) * Mathf.Deg2Rad;
            Vector2 newPos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

            int buttonIndex = newLogicalOrder[i];
            targetPositions[buttonIndex] = centerButton.anchoredPosition + newPos;
            targetSizes[buttonIndex] = expandedSizes[i];
        }
    }

    private void UpdateTextDisplay()
    {
        if (panelText != null && abilityTexts != null && abilityTexts.Length > currentCenterIndex)
        {
            // Hide all texts first
            for (int i = 0; i < abilityTexts.Length; i++)
            {
                abilityTexts[i].gameObject.SetActive(false);
            }

            // Show the current center ability text
            int centerAbilityIndex = currentButtonOrder[2];
            abilityTexts[centerAbilityIndex].gameObject.SetActive(true);
            panelText.SetActive(true);
        }
    }

    public void AnimateLayout(float targetRadius, float targetOffset, float targetSpacing, bool toHalfCircle)
    {
        if (layoutRoutine != null)
            StopCoroutine(layoutRoutine);

        layoutRoutine = StartCoroutine(AnimateLayoutRoutine(targetRadius, targetOffset, targetSpacing, toHalfCircle));
    }

    private IEnumerator AnimateLayoutRoutine(float targetRadius, float targetOffset, float targetSpacing, bool toHalfCircle)
    {
        float startRadius = radius;
        float startOffset = angleOffset;
        float startSpacing = spacing;

        float angleStart = toHalfCircle ? 360f * startSpacing : 180f * startSpacing;
        float angleEnd = toHalfCircle ? 180f * targetSpacing : 360f * targetSpacing;

        Vector2[] startSizes = new Vector2[buttons.Length];
        Vector2[] targetSizes = new Vector2[buttons.Length];

        for (int i = 0; i < buttons.Length; i++)
        {
            startSizes[i] = buttons[i].sizeDelta;
            targetSizes[i] = toHalfCircle ? expandedSizes[i] : collapsedSize;
        }

        float elapsed = 0f;

        while (elapsed < layoutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / layoutDuration;

            radius = Mathf.Lerp(startRadius, targetRadius, t);
            angleOffset = Mathf.Lerp(startOffset, targetOffset, t);
            spacing = Mathf.Lerp(startSpacing, targetSpacing, t);

            float angleRange = Mathf.Lerp(angleStart, angleEnd, t);

            ApplyLayout(angleRange);

            for (int i = 0; i < buttons.Length; i++)
            {
                Vector2 newSize = Vector2.Lerp(startSizes[i], targetSizes[i], t);
                buttons[i].sizeDelta = newSize;
            }

            yield return null;
        }

        radius = targetRadius;
        angleOffset = targetOffset;
        spacing = targetSpacing;

        ApplyLayout(toHalfCircle ? 180f * spacing : 360f * spacing);

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].sizeDelta = toHalfCircle ? expandedSizes[i] : collapsedSize;
        }

        if (toHalfCircle)
        {
            ApplyLayout(180f * spacing);
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].sizeDelta = expandedSizes[i];
            }
        }

        if (toHalfCircle)
        {
            UpdateTextDisplay();
        }
        else
        {
            if (panelText != null)
                panelText.SetActive(false);
        }
    }

    public void UpdateButtonArrayOrder()
    {
        RectTransform[] newButtonArray = new RectTransform[buttons.Length];

        for (int position = 0; position < 5; position++)
        {
            int abilityIndex = currentButtonOrder[position];
            newButtonArray[position] = originalButtons[abilityIndex];
        }

        buttons = newButtonArray;

        currentCenterIndex = 2;
    }

    public void SaveCurrentButtonOrder()
    {
        hasSavedOrder = true;
        UpdateButtonArrayOrder();
    }

    public void RestorePreviousCenter(int savedIndex)
    {
        currentCenterIndex = 2; // Center is always at position 2 now
        UpdateTextDisplay();

        // Apply proper layout and sizes
        ApplyLayout();
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].sizeDelta = expandedSizes[i];
        }
    }

    private void ApplyLayout(float angleRange = -1f)
    {
        if (angleRange < 0f)
            angleRange = 360f * spacing;

        int count = buttons.Length;
        float angleStep = (count > 1) ? angleRange / count : 0f;

        for (int i = 0; i < count; i++)
        {
            float angle = (angleOffset + i * angleStep) * Mathf.Deg2Rad;
            Vector2 newPos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            buttons[i].anchoredPosition = centerButton.anchoredPosition + newPos;
        }
    }

    public void RestorePreviousLayout(int savedIndex)
    {
        if (!hasSavedOrder) return;

        RectTransform[] newButtonArray = new RectTransform[buttons.Length];
        for (int i = 0; i < 5; i++)
        {
            int buttonIndex = currentButtonOrder[i];
            newButtonArray[i] = originalButtons[buttonIndex];
        }

        buttons = newButtonArray;
        currentCenterIndex = savedIndex;

        ApplyLayout();
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].sizeDelta = expandedSizes[i];
        }

        UpdateTextDisplay();
    }

    public int GetCurrentCenterIndex()
    {
        return currentCenterIndex;
    }

    public void UpdateTextManually()
    {
        UpdateTextDisplay();
    }

}