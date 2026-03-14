using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;


public class EnemyIntroductionUI : BaseIntroductionUI, IHasPadLock
{

    public static EnemyIntroductionUI Instance { get; private set; }

    public event EventHandler<IHasPadLock.UnlockPadLockEventArgs> UnlockPadLock;

    [Header("Enemy Data")] 
    [SerializeField] private EnemySO enemySO;
    [SerializeField] private Image enemyImage;

    [Header("Button")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button anomalyButton;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI attackDamageText;
    [SerializeField] private TextMeshProUGUI attackTimerText;
    [SerializeField] private TextMeshProUGUI enemyDescription;
    [SerializeField] private TextMeshProUGUI anomalyDescriptionText;

    [Header("DOTween")]
    [SerializeField] private float dropDuration;
    [SerializeField] private float startPosY;
    [SerializeField] private Vector3 punchStrength;
    [SerializeField] private float punchDuration;

    [Header("AnomalyUI")]
    [SerializeField] private RectTransform anomalyUIRect;
    [SerializeField] private Vector3 startPoint;
    [SerializeField] private Vector3 endPoint;
    [SerializeField] private float showDuration;

    [Header("Anomaly Introduction Anim")]
    [SerializeField] private Image anomalyImage;
    [SerializeField] private AnomalyIntroductionAnimator anomalyIntroductionAnimator;

    private RectTransform rectTransform;

    private Action actionCallback = null;

    private bool isShowAnomalyUI;
    private Coroutine anomalyIntroAnimCoroutine;

    private void Awake() {

        Instance = this;

        rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0, startPosY);

        isShowAnomalyUI = false;    

        // Button
        closeButton.onClick.AddListener(() => {

            StartCoroutine(HideCoroutine());

        });

        anomalyButton.onClick.AddListener(() => {
            ToggleAnomalyUI();
        });

    }

    private void Start() {

        StartCoroutine(UpdateVisualCoroutine());

    }

    private IEnumerator UpdateVisualCoroutine() {

        Time.timeScale = 0f;

        closeButton.interactable = false;

        yield return new WaitForEndOfFrame(); // Wait 1 frame to ensure data and enemy status has updated

        // 1. Update local data
        UpdateVisual();

        anomalyButton.interactable = false;

        // 2. DOTween animation
        rectTransform.anchoredPosition = new Vector2(0, startPosY);

        Sequence enemySequence = DOTween.Sequence().SetUpdate(true);

        enemySequence.Append(
            rectTransform.DOAnchorPosY(0, dropDuration).SetEase(Ease.InQuad)
        );

        enemySequence.Append(
            rectTransform.DOPunchScale(punchStrength, punchDuration)
        );

        yield return enemySequence.WaitForCompletion();

        // 3. Unlock PadLock animation - If is introduced new anomaly
        if (!HasAnomaly()){

            closeButton.interactable = true;
        }

        if (SaveData.IsUnlockAnomaly(enemySO)) {

            UnlockPadLock?.Invoke(this, new IHasPadLock.UnlockPadLockEventArgs {
                callbackAction = () => {

                    anomalyButton.interactable = true;
                    closeButton.interactable = true;
                }
            });
        }
        else {
            closeButton.interactable = true;
        }

    }

    private IEnumerator HideCoroutine() {

        closeButton.interactable = false;

        // 1. First Pos
        rectTransform.anchoredPosition = Vector3.zero;

        if (isShowAnomalyUI) {
            ToggleAnomalyUI();

            yield return new WaitForSecondsRealtime(showDuration);
        }

        yield return new WaitForEndOfFrame();

        // 2. DOTween progress
        Sequence enemySequence = DOTween.Sequence().SetUpdate(true);

        enemySequence.Append(
            rectTransform.DOPunchScale(punchStrength, punchDuration)
        );

        enemySequence.Append(
            rectTransform.DOAnchorPosY(startPosY, dropDuration).SetEase(Ease.InQuad)
        );

        yield return enemySequence.WaitForCompletion();

        actionCallback?.Invoke();

        LevelManager.Instance.SetIsShowingEnemyIntroductionUI(false);

        Time.timeScale = 1f;

        Destroy(this.gameObject);

    }

    private IEnumerator AnomalyIntroAnimCoroutine() {

        float changeBehaviorTimer = anomalyIntroductionAnimator.changeBehaviorTimer;
        float frameRate = anomalyIntroductionAnimator.frameRate;

        while (isShowAnomalyUI) {
            // While show anomalyUi is On ---> Run loop

            int currentFrame = 0;

            // Local Behvaior
            int countLoop = 0;
            List<Sprite> localBehaviorList = anomalyIntroductionAnimator.localBehaviorList;

            while (countLoop < 5) {

                currentFrame = 0;

                while (currentFrame < localBehaviorList.Count) {

                    anomalyImage.sprite = localBehaviorList[currentFrame];
                    currentFrame += 1;

                    yield return new WaitForSecondsRealtime(frameRate);
                }

                countLoop += 1;
                yield return new WaitForSecondsRealtime(frameRate);
            }
            

            yield return new WaitForSecondsRealtime(changeBehaviorTimer);

            // Anomaly Behavior
            currentFrame = 0;
            List<Sprite> anomalyBehaviorList = anomalyIntroductionAnimator.anomalyBehavior;

            while (currentFrame < anomalyBehaviorList.Count) {

                anomalyImage.sprite = anomalyBehaviorList[currentFrame];
                currentFrame += 1;

                yield return new WaitForSecondsRealtime(frameRate);
            }

            yield return new WaitForSecondsRealtime(1f);
        }

    }

    private void UpdateVisual() {

        // Introduction UI
        titleText.text = $"new enemy: {enemySO.enemyName}";
        healthText.text = $"{enemySO.totalHealth}";
        speedText.text = $"{enemySO.moveSpeed}";
        enemyDescription.text = $"{enemySO.description}";

        // Anomaly UI
        if (HasAnomaly()) {

            string attackValue;
            string attackTimerValue;

            if (enemySO.attackDamage == 0f && enemySO.attackTimer == 0f) {
                attackValue = "none";
                attackTimerValue = "none";
            }
            else {
                attackValue = $"{enemySO.attackDamage}";
                attackTimerValue = $"{enemySO.attackTimer}";
            }

            attackDamageText.text = attackValue;
            attackTimerText.text = attackTimerValue;
        }
    }

    private void ToggleAnomalyUI() {

        isShowAnomalyUI = !isShowAnomalyUI;
        
        if (isShowAnomalyUI) {

            anomalyUIRect.transform.gameObject.SetActive(true);

            UpdateVisual();

            // 0. Anomaly Introduction Animator
            if (anomalyIntroAnimCoroutine != null) {
                StopCoroutine(anomalyIntroAnimCoroutine);
                anomalyIntroAnimCoroutine = null;
            }

            anomalyIntroAnimCoroutine = StartCoroutine(AnomalyIntroAnimCoroutine());

            // 1. First Pos
            anomalyUIRect.anchoredPosition = startPoint;
            anomalyDescriptionText.text = "";

            // 2. DOTween anim happen
            Sequence anomalyUIShowSequence = DOTween.Sequence().SetUpdate(true);

            anomalyUIShowSequence.Append(anomalyUIRect.DOAnchorPos(endPoint, showDuration).SetEase(Ease.OutBack));

            string fullText = $"{enemySO.anomalyDescription}"; 

            anomalyUIShowSequence.Append(DOTween.To(() => anomalyDescriptionText.text,x => anomalyDescriptionText.text = x,fullText, 0.5f).SetEase(Ease.Linear));

        }
        else {

            // 1. First Pos
            anomalyUIRect.anchoredPosition = endPoint;

            // 2. DOTween anim happen
            Sequence anomalyUIHideSequence = DOTween.Sequence().SetUpdate(true);
            anomalyUIHideSequence.Append(anomalyUIRect.DOAnchorPos(startPoint, showDuration).SetEase(Ease.InBack)).OnComplete(() => {

                anomalyUIRect.transform.gameObject.SetActive(false);
            });

        }
    }

    private bool HasAnomaly() {
        return anomalyUIRect.transform.gameObject.activeSelf;
    }

    public static void SpawnEnemyIntroductionUI(EnemySO enemySO, Transform parent, Action actionCallback = null) {

        Transform introduction = Instantiate(enemySO.introductionUI, parent);

        EnemyIntroductionUI enemyIntroductionUI = introduction.GetComponent<EnemyIntroductionUI>();

        // When spawn IntroductionUI
        if (!SaveData.HasIntroduced(enemySO)) {
            // Nếu chưa từng introduce về enemy này

            SaveData.SetEnemyIntroduced(enemySO);
        }
        else {
            // Nếu đã introduce về enemy này

            if (!SaveData.IsUnlockAnomaly(enemySO)) {
                // Nếu chưa unlock anomaly của enemy này

                SaveData.UnlockAnomalyEnemy(enemySO);
            }
            else {
                // Nếu đã unlock anomaly của enemy này
            }
        }

        if (actionCallback != null) {
            enemyIntroductionUI.actionCallback = actionCallback;
        }
    }
}

[System.Serializable]
public class AnomalyIntroductionAnimator {

    public List<Sprite> localBehaviorList = new List<Sprite>();
    public List<Sprite> anomalyBehavior = new List<Sprite>();
    public float changeBehaviorTimer;
    public float frameRate;
}