using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CampaignMapUI : MonoBehaviour
{
    public static CampaignMapUI Instance { get; private set; }

    public event EventHandler WorldMapScrollDownSFX;
    public event EventHandler WorldMapScrollUpSFX;
    public event EventHandler ButtonClickedSFX;

    [Header("Scene UI Data")]
    [SerializeField] private SceneMangerSO sceneMangerSO;

    [Header("RectTransform Image")]
    [SerializeField] private RectTransform worldMap;
    [SerializeField] private RectTransform woodenRollerLeft;
    [SerializeField] private RectTransform woodenRollerRight;

    [Header("Moving Behavior")]
    [SerializeField] private Vector3 startPointRollerLeft;
    [SerializeField] private Vector3 endPointRollerLeft;
    [SerializeField] private Vector3 startPointRollerRight;
    [SerializeField] private Vector3 endPointRollerRight;
    [SerializeField] private float minWidthWorldMap;
    [SerializeField] private float maxWidthWorldMap;

    [Header("CanvasGroup")]
    [SerializeField] private CanvasGroup mapImage;
    [SerializeField] private CanvasGroup biomeTextCanvas;

    [Header("Function Button")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button upgradeAbilityButton;
    [SerializeField] private Button dataTowerButton;


    [Header("Level Button")]
    [SerializeField] private Transform levelButtonPrefab;
    [SerializeField] private CanvasGroup levelButtonParent;
    
    [Header("Level Anchor Point")]
    [SerializeField] private LevelAnchorPoint[] levelAnchorPointArray;

    [SerializeField] private TextMeshProUGUI totalPointText;

    private CanvasGroup mainCanvasGroup;

    private Dictionary<ILevelManager.BiomeType, List<LevelAnchorPoint>> anchorPointDict;

    private int totalPoint;

    private void Awake() {

        Instance = this;

        mainCanvasGroup = GetComponent<CanvasGroup>();

        closeButton.onClick.AddListener(() => {
            StartCoroutine(DespawnCoroutine());
        });

        upgradeAbilityButton.onClick.AddListener(() => {

            ButtonClickedSFX?.Invoke(this, EventArgs.Empty);

            Instantiate(sceneMangerSO.upgradeAbilityUI, this.transform);
        });

        dataTowerButton.onClick.AddListener(() => {
            ButtonClickedSFX?.Invoke(this, EventArgs.Empty);

            Instantiate(sceneMangerSO.dataTowerUI, this.transform);
        });

        anchorPointDict = new Dictionary<ILevelManager.BiomeType, List<LevelAnchorPoint>>();

        foreach (LevelAnchorPoint levelAnchorPoint in levelAnchorPointArray) {

            if (!anchorPointDict.ContainsKey(levelAnchorPoint.GetBiomeType())) {

                anchorPointDict[levelAnchorPoint.GetBiomeType()] = new List<LevelAnchorPoint>();

            }
            anchorPointDict[levelAnchorPoint.GetBiomeType()].Add(levelAnchorPoint);
        }

    }

    private void Start() {

        // Spawn Button Level Base on anchorPointData
        foreach (var key in anchorPointDict.Keys) {

            foreach (LevelAnchorPoint levelAnchorPoint in anchorPointDict[key]) {

                LevelButton levelButton = LevelButton.SpawnLevelButton(levelButtonPrefab, levelButtonParent.transform, levelAnchorPoint);

            }

        }

        // After Spawn
        StartCoroutine(SpawnCoroutine());

        SetTotalPoint(SaveData.GetGameStatus().totalPoints);
    }

    private IEnumerator SpawnCoroutine() {

        mainCanvasGroup.interactable = false;

        // 1. First Setup
        woodenRollerLeft.anchoredPosition = startPointRollerLeft;
        woodenRollerRight.anchoredPosition = startPointRollerRight;
        worldMap.sizeDelta = new Vector2(minWidthWorldMap, worldMap.sizeDelta.y);

        mapImage.alpha = 0f;
        biomeTextCanvas.alpha = 0f;
        levelButtonParent.alpha = 0f;

        // 2. Fade In Main Canvas
        float fadeDuration = 0.1f;
        mainCanvasGroup.alpha = 0f;

        yield return mainCanvasGroup.DOFade(1f, fadeDuration).SetEase(Ease.Linear).WaitForCompletion();

        // 3. Scroll Down World Map
        WorldMapScrollDownSFX?.Invoke(this, EventArgs.Empty);

        float scrollTimer = 0f;
        float scrollTimerMax = 1f;

        while (scrollTimer <= scrollTimerMax) {

            float elapsed = Mathf.Clamp01(scrollTimer / scrollTimerMax);
            float smoothT = Mathf.SmoothStep(0f, 1f, elapsed);

            // Wooden Roller
            woodenRollerLeft.anchoredPosition = Vector3.Lerp(startPointRollerLeft, endPointRollerLeft, smoothT);
            woodenRollerRight.anchoredPosition = Vector3.Lerp(startPointRollerRight, endPointRollerRight, smoothT);

            // World Map
            worldMap.sizeDelta = new Vector2(Mathf.Lerp(minWidthWorldMap, maxWidthWorldMap, smoothT), worldMap.sizeDelta.y);
            mapImage.alpha = smoothT;

            scrollTimer += Time.deltaTime;

            yield return null;
        }

        woodenRollerLeft.anchoredPosition = endPointRollerLeft;
        woodenRollerRight.anchoredPosition = endPointRollerRight;
        
        worldMap.sizeDelta = new Vector2(maxWidthWorldMap, worldMap.sizeDelta.y);
        mapImage.alpha = 1f;

        // 4. Fade In BiomeText and LevelButton
        float duration = 0.1f;
        Sequence fadeOutDataSequence = DOTween.Sequence();

        fadeOutDataSequence.Append(biomeTextCanvas.DOFade(1f, duration).SetEase(Ease.Linear));
        fadeOutDataSequence.Join(levelButtonParent.DOFade(1f, duration).SetEase(Ease.Linear));

        fadeOutDataSequence.OnComplete(() => {

            mainCanvasGroup.interactable = true;
        });
    }

    private IEnumerator DespawnCoroutine() {

        mainCanvasGroup.interactable = false;

        // 1. Fade Out BiomeText and LevelButton
        float duration = 0.1f;
        Sequence fadeOutDataSequence = DOTween.Sequence();

        fadeOutDataSequence.Append(biomeTextCanvas.DOFade(0f, duration).SetEase(Ease.Linear));
        fadeOutDataSequence.Join(levelButtonParent.DOFade(0f, duration).SetEase(Ease.Linear));

        yield return fadeOutDataSequence.WaitForCompletion();

        // 2. Scroll Up World Map
        WorldMapScrollUpSFX?.Invoke(this, EventArgs.Empty);

        float scrollTimerMax = 1f;
        float scrollTimer = scrollTimerMax;

        while (scrollTimer >= 0f) {

            float elapsed = Mathf.Clamp01(scrollTimer / scrollTimerMax);
            float smoothT = Mathf.SmoothStep(0f, 1f, elapsed);

            // Wooden Roller
            woodenRollerLeft.anchoredPosition = Vector3.Lerp(startPointRollerLeft, endPointRollerLeft, smoothT);
            woodenRollerRight.anchoredPosition = Vector3.Lerp(startPointRollerRight, endPointRollerRight, smoothT);

            // World Map
            worldMap.sizeDelta = new Vector2(Mathf.Lerp(minWidthWorldMap, maxWidthWorldMap, smoothT), worldMap.sizeDelta.y);
            mapImage.alpha = smoothT;

            scrollTimer -= Time.deltaTime;

            yield return null;
        }

        // 3. Fade Out Main Canvas
        float fadeDuration = 0.1f;

        mainCanvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.Linear).OnComplete(() => {
            Destroy(this.gameObject);
        });

    }

    public int GetTotalPoint() {
        return this.totalPoint;
    }

    public void SetTotalPoint(int toltalPoint) {
        
        this.totalPoint = toltalPoint;

        totalPointText.text = $": {this.totalPoint}";

        SaveData.SetNewTotalPoint(this.totalPoint);
    }

}
