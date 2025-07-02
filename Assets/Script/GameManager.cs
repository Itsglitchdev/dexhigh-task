using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Main Button Settings")]
    [SerializeField] private RectTransform mainButton;
    [SerializeField] private Vector2 startPos = new Vector2(-75, 75);
    [SerializeField] private Vector2 targetPos = new Vector2(-162, 172);
    [SerializeField] private Vector2 startScale = new Vector2(150, 150);
    [SerializeField] private Vector2 targetScale = new Vector2(250, 250);
    [SerializeField] private float animDuration = 0.5f;

    [Header("Collapsed Layout")]
    [SerializeField] private float collapsedRadius = 70f;
    [SerializeField] private float collapsedOffset = 16.5f;
    [SerializeField] private float collapsedSpacing = 1f;

    [Header("Expanded Layout")]
    [SerializeField] private float expandedRadius = 90f;
    [SerializeField] private float expandedOffset = 85f;
    [SerializeField] private float expandedSpacing = 0.65f;

    [Header("Background")]
    [SerializeField] private CanvasGroup backgroundCanvasGroup;
    [SerializeField] private GameObject backgroundPanel;

    [Header("Ability Button Layout")]
    [SerializeField] private AbilityButtonsLayout buttonsLayout;

    private bool isExpanded = false;
    private Coroutine animationRoutine;

    // ✅ Track the center ability index
    private int savedCenterIndex = 2;

    void Start()
    {
        backgroundPanel.SetActive(false);

        // Always restore latest saved center index and button order on start
        savedCenterIndex = buttonsLayout.GetCurrentCenterIndex();

        // Optional: Uncomment this if you want to open by default
        // RestoreStateAndExpand(); 
    }


    public void OnMainButtonClick()
    {
        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        animationRoutine = StartCoroutine(ToggleMainButton());
    }

    private IEnumerator ToggleMainButton()
    {
        Vector2 fromPos = isExpanded ? targetPos : startPos;
        Vector2 toPos = isExpanded ? startPos : targetPos;
        Vector2 fromScale = isExpanded ? targetScale : startScale;
        Vector2 toScale = isExpanded ? startScale : targetScale;

        float fromAlpha = isExpanded ? 1 : 0;
        float toAlpha = isExpanded ? 0 : 1;

        backgroundPanel.SetActive(true);

        if (!isExpanded)
        {
            buttonsLayout.RestorePreviousLayout(savedCenterIndex);
            buttonsLayout.AnimateLayout(expandedRadius, expandedOffset, expandedSpacing, true);
            buttonsLayout.UpdateTextManually();
        }
        else
        {
            savedCenterIndex = buttonsLayout.GetCurrentCenterIndex();
            buttonsLayout.SaveCurrentButtonOrder();

            buttonsLayout.AnimateLayout(collapsedRadius, collapsedOffset, collapsedSpacing, false);
        }

        // Animate the main button and background fade
        float time = 0f;
        while (time < animDuration)
        {
            time += Time.deltaTime;
            float t = time / animDuration;

            mainButton.anchoredPosition = Vector2.Lerp(fromPos, toPos, t);
            mainButton.sizeDelta = Vector2.Lerp(fromScale, toScale, t);
            backgroundCanvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);

            yield return null;
        }

        mainButton.anchoredPosition = toPos;
        mainButton.sizeDelta = toScale;
        backgroundCanvasGroup.alpha = toAlpha;

        if (toAlpha == 0)
            backgroundPanel.SetActive(false);

        isExpanded = !isExpanded;
    }

}