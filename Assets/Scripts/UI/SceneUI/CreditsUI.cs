using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreditsUI : MonoBehaviour
{
    public static CreditsUI Instance { get; private set; }

    public event EventHandler AncientPaperScrollDownSFX;
    public event EventHandler AncientPaperScrollUpSFX;

    [Header("RectTransform Image")]
    [SerializeField] private RectTransform ancientEmptyPaper;
    [SerializeField] private RectTransform woodenRollerLeft;
    [SerializeField] private RectTransform woodenRollerRight;

    [Header("Moving Behavior")]
    [SerializeField] private Vector3 startPointRollerLeft;
    [SerializeField] private Vector3 endPointRollerLeft;
    [SerializeField] private Vector3 startPointRollerRight;
    [SerializeField] private Vector3 endPointRollerRight;
    [SerializeField] private float minWidthWorldMap;
    [SerializeField] private float maxWidthWorldMap;

    [Header("Function Button")]
    [SerializeField] private Button closeButton;

    private CanvasGroup mainCanvasGroup;

    private void Awake() {

        Instance = this;    

        mainCanvasGroup = GetComponent<CanvasGroup>();

        closeButton.onClick.AddListener(() => {

            StartCoroutine(DespawnCoroutine());
        });
    }

    private void Start() {

        // After Spawn
        StartCoroutine(SpawnCoroutine());

    }

    private IEnumerator SpawnCoroutine() {

        mainCanvasGroup.interactable = false;

        // 1. First Setup
        woodenRollerLeft.anchoredPosition = startPointRollerLeft;
        woodenRollerRight.anchoredPosition = startPointRollerRight;
        ancientEmptyPaper.sizeDelta = new Vector2(minWidthWorldMap, ancientEmptyPaper.sizeDelta.y);

        // 2. Fade In Main Canvas
        float fadeDuration = 0.1f;
        mainCanvasGroup.alpha = 0f;

        yield return mainCanvasGroup.DOFade(1f, fadeDuration).SetEase(Ease.Linear).WaitForCompletion();

        // 3. Scroll Down Map
        AncientPaperScrollDownSFX?.Invoke(this, EventArgs.Empty);

        float scrollTimer = 0f;
        float scrollTimerMax = 1f;

        while (scrollTimer <= scrollTimerMax) {

            float elapsed = Mathf.Clamp01(scrollTimer / scrollTimerMax);
            float smoothT = Mathf.SmoothStep(0f, 1f, elapsed);

            // Wooden Roller
            woodenRollerLeft.anchoredPosition = Vector3.Lerp(startPointRollerLeft, endPointRollerLeft, smoothT);
            woodenRollerRight.anchoredPosition = Vector3.Lerp(startPointRollerRight, endPointRollerRight, smoothT);

            // World Map
            ancientEmptyPaper.sizeDelta = new Vector2(Mathf.Lerp(minWidthWorldMap, maxWidthWorldMap, smoothT), ancientEmptyPaper.sizeDelta.y);

            scrollTimer += Time.deltaTime;

            yield return null;
        }

        woodenRollerLeft.anchoredPosition = endPointRollerLeft;
        woodenRollerRight.anchoredPosition = endPointRollerRight;

        ancientEmptyPaper.sizeDelta = new Vector2(maxWidthWorldMap, ancientEmptyPaper.sizeDelta.y);

        // 4. Todo: Fade In Credit
        mainCanvasGroup.interactable = true;
    }

    private IEnumerator DespawnCoroutine() {

        mainCanvasGroup.interactable = false;

        // 1. Todo: Fade Out Credit

        yield return null;

        // 2. Scroll Up Map
        AncientPaperScrollUpSFX?.Invoke(this, EventArgs.Empty);

        float scrollTimerMax = 1f;
        float scrollTimer = scrollTimerMax;

        while (scrollTimer >= 0f) {

            float elapsed = Mathf.Clamp01(scrollTimer / scrollTimerMax);
            float smoothT = Mathf.SmoothStep(0f, 1f, elapsed);

            // Wooden Roller
            woodenRollerLeft.anchoredPosition = Vector3.Lerp(startPointRollerLeft, endPointRollerLeft, smoothT);
            woodenRollerRight.anchoredPosition = Vector3.Lerp(startPointRollerRight, endPointRollerRight, smoothT);

            // World Map
            ancientEmptyPaper.sizeDelta = new Vector2(Mathf.Lerp(minWidthWorldMap, maxWidthWorldMap, smoothT), ancientEmptyPaper.sizeDelta.y);

            scrollTimer -= Time.deltaTime;

            yield return null;
        }

        // 3. Fade Out Main Canvas
        float fadeDuration = 0.1f;

        mainCanvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.Linear).OnComplete(() => {
            Destroy(this.gameObject);
        });

    }
}
