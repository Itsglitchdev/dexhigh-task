using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button mainButton;

    [Header("Main Button")]
    [SerializeField] private RectTransform mainButtonRect;
    [SerializeField] private Vector2 initialPos = Vector2.zero;
    [SerializeField] private Vector2 initialSize = new Vector2(150, 150);
    [SerializeField] private Vector2 expandedPos = new Vector2(-80, 100);
    [SerializeField] private Vector2 expandedSize = new Vector2(250, 250);
    [SerializeField] private float animationDuration = 0.5f;

    [Header("Background UI")]
    [SerializeField] private CanvasGroup backgroundCanvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Ability Button Layout")]
    [SerializeField] private AbilityButtonsFinal abilityButtonsLayout;
    [SerializeField] private float expandedRadius = 95f;
    [SerializeField] private float expandedAngle = 230f;
    [SerializeField] private float collapsedRadius = 70f;
    [SerializeField] private float collapsedAngle = 360f;

    private bool isExpanded = false;

    void Start()
    {
        backgroundCanvasGroup.alpha = 0f;
        ButtonEventHandler();
        AbilityButtonInteractibility();
    }

    void ButtonEventHandler()
    {
        mainButton.onClick.AddListener(OnMainButtonClick);
    }

    void AbilityButtonInteractibility()
    {
        foreach (Transform child in abilityButtonsLayout.transform)
        {
            Button btn = child.GetComponent<Button>();
            if (btn != null)
                btn.interactable = isExpanded;
        }
    }

    void OnMainButtonClick()
    {
        isExpanded = !isExpanded;

        StopAllCoroutines();
        StartCoroutine(AnimateMainButton(
            isExpanded ? expandedPos : initialPos,
            isExpanded ? expandedSize : initialSize,
            animationDuration
        ));

        StartCoroutine(FadeCanvasGroup(backgroundCanvasGroup, isExpanded ? 1 : 0, fadeDuration));

        if (abilityButtonsLayout != null)
        {
            float radius = isExpanded ? expandedRadius : collapsedRadius;
            float angle = isExpanded ? expandedAngle : collapsedAngle;

            abilityButtonsLayout.SetLayout(radius, angle);

            AbilityButtonInteractibility();
        }
    }

    private IEnumerator AnimateMainButton(Vector2 targetPos, Vector2 targetSize, float duration)
    {
        Vector2 startPos = mainButtonRect.anchoredPosition;
        Vector2 startSize = mainButtonRect.sizeDelta;

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            mainButtonRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            mainButtonRect.sizeDelta = Vector2.Lerp(startSize, targetSize, t);

            yield return null;
        }

        mainButtonRect.anchoredPosition = targetPos;
        mainButtonRect.sizeDelta = targetSize;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float targetAlpha, float duration)
    {
        float startAlpha = group.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            group.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }

        group.alpha = targetAlpha;
        group.interactable = targetAlpha > 0.9f;
        group.blocksRaycasts = targetAlpha > 0.9f;
    }
}
