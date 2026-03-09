using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CatapultTowerUpgradeNotifyUI : MonoBehaviour
{

    [SerializeField] private CatapultTower catapultTower;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI oldAttackDamageValueText;
    [SerializeField] private TextMeshProUGUI newAttackDamageValueText;
    [SerializeField] private TextMeshProUGUI oldAttackRangeValueText;
    [SerializeField] private TextMeshProUGUI newAttackRangeValueText;
    [SerializeField] private TextMeshProUGUI oldCooldownValueText;
    [SerializeField] private TextMeshProUGUI newCooldownText;
    [SerializeField] private TextMeshProUGUI oldSplashRadiusText;
    [SerializeField] private TextMeshProUGUI newSplashRadiusText;



    [Header("Dotween")]
    [SerializeField] private Vector3 minScale;
    [SerializeField] private Vector3 maxScale;
    [SerializeField] private float duration;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private void Awake() {

        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.interactable = false;

    }

    private void Start() {

        catapultTower.UnUpgradeLevelProgress += GuardianTower_UnUpgradeLevelProgress;


        // After Spawn
        Hide();
    }

    private void OnDestroy() {

        catapultTower.UnUpgradeLevelProgress -= GuardianTower_UnUpgradeLevelProgress;
    }

    private void GuardianTower_UnUpgradeLevelProgress(object sender, System.EventArgs e) {
        // After Upgrade Progress done

        if (catapultTower.GetCurrentTowerStatus().levelTower == ITowerObject.LevelTower.Level1) {
            return;
        }

        Show();
        UpdateVisual();

        ShowCoroutine();
    }

    private void ShowCoroutine() {


        float waitTimer = 1.5f;

        // 1. Setup
        rectTransform.localScale = minScale;
        canvasGroup.alpha = 0f;

        Sequence notifySequence = DOTween.Sequence();

        notifySequence.Append(rectTransform.DOScale(maxScale, duration).SetEase(Ease.OutBack));
        notifySequence.Join(canvasGroup.DOFade(1f, duration).SetEase(Ease.Linear));

        notifySequence.AppendInterval(waitTimer);

        notifySequence.Append(rectTransform.DOScale(minScale, duration).SetEase(Ease.InBack));
        notifySequence.Join(canvasGroup.DOFade(0f, duration / 2).SetEase(Ease.Linear)).OnComplete(() => {

            Hide();
        });


    }

    private void UpdateVisual() {

        CatapultTowerLevelData currentLevelData = catapultTower.GetCurrentTowerStatus();
        CatapultTowerLevelData oldLevelData = catapultTower.GetCatapulTowerDataDict()[(ITowerObject.LevelTower)((int)currentLevelData.levelTower - 1)];


        // 1. Attack damage
        oldAttackDamageValueText.text = $"{oldLevelData.attackDamage}";
        newAttackDamageValueText.text = $"{currentLevelData.attackDamage}";

        // 2. Attack range
        float oldAttackRangeValue;

        if (oldLevelData.levelTower == ITowerObject.LevelTower.Level1) {

            oldAttackRangeValue = catapultTower.GetCatapultTowerSO().baseRange;
        }
        else {
            oldAttackRangeValue = oldLevelData.attackRange;
        }

        oldAttackRangeValueText.text = $"{oldAttackRangeValue}";
        newAttackRangeValueText.text = $"{currentLevelData.attackRange}";

        // 3. Attack Speed
        oldCooldownValueText.text = $"{oldLevelData.cooldownTimer}";
        newCooldownText.text = $"{currentLevelData.cooldownTimer}";

        // 4. Training Time
        oldSplashRadiusText.text = $"{oldLevelData.pebbleSplashRadius}";
        newSplashRadiusText.text = $"{currentLevelData.pebbleSplashRadius}";
    }

    private void Show() {
        this.gameObject.SetActive(true);
    }

    private void Hide() {
        this.gameObject.SetActive(false);
    }
}
