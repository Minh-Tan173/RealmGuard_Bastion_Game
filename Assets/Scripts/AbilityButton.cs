using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilityButton : MonoBehaviour, IHasClockTimer, IPointerEnterHandler, IPointerExitHandler {

    public event EventHandler<IHasClockTimer.OnChangeProgressEventArgs> OnChangeProgress;

    [Header("Ability Type")]
    [SerializeField] private IAbility.AbilityType abilityType;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI abilityPriceText;
    [SerializeField] private TextMeshProUGUI readyNotiText;

    [Header("Introduction UI")]
    [SerializeField] private RectTransform abilityIntroductionUI;
    [SerializeField] private CanvasGroup abilityIntroductionUICanvas;
    [SerializeField] private float scaleValue;

    private bool isCanUseAbility;
    private AbilitySO abilitySO;

    private Button button;

    private void Awake() {

        button = GetComponent<Button>();

        button.onClick.AddListener(() => {
            BuyAbility();
        });

    }

    private void Start() {

        BuildingManager.Instance.CreateAbilityDone += BuildingManager_CreateAbilityDone;

        abilitySO = SaveData.GetAbilitySOByType(abilityType);

        abilityPriceText.text = $"{abilitySO.price}";

        abilityIntroductionUI.transform.gameObject.SetActive(false);

        // When start game

        if (SaveData.GetAbilityStatusByType(abilityType).isLocked) {
            // Not unlock this ability;
            this.gameObject.SetActive(false);
        }
        else {
            // Unlock this ability
            this.gameObject.SetActive(true);

            StartCoroutine(CDCouroutine());
        }

    }

    private void OnDestroy() {
        BuildingManager.Instance.CreateAbilityDone -= BuildingManager_CreateAbilityDone;
    }


    private void BuildingManager_CreateAbilityDone(object sender, BuildingManager.CreateAbilityDoneEventArgs e) {
        // After create ability

        if (e.abilityType == this.abilityType) {

            StartCoroutine(CDCouroutine());

        }

    }

    private void BuyAbility() {

        if (!IsCanUseAbility()) {
            return;
        }

        if (BuildingManager.Instance.IsPlacingAbility() || BuildingManager.Instance.IsPlacingTower()) {
            BuildingManager.Instance.ForceReset();
            return;
        }


        if (LevelManager.Instance.GetCurrentCoin() >= abilitySO.price) {

            BuildingManager.Instance.SpawnAbilityIcon(abilitySO);

            LevelManager.Instance.ChangedCoinTo(ILevelManager.CoinChangedState.Decrase, abilitySO.price);

        }
        else {
            // Dont have enough money
        }

    }

    private IEnumerator CDCouroutine() {

        HideNoti();

        isCanUseAbility = false;

        IAbility.AbilityLevel currentAbilityLevel = SaveData.GetAbilityStatusByType(abilitySO.abilityType).currentLevel;
        float timer = SaveData.GetAbilityLevelDataByLevelAndType(currentAbilityLevel, abilitySO.abilityType).cdTimer;

        float cdTimer = timer;
        float cdTimerMax = timer;

        OnChangeProgress?.Invoke(this, new IHasClockTimer.OnChangeProgressEventArgs { progressNormalized = 1f });

        yield return null;

        while (cdTimer >= 0f) {

            float t = Mathf.Clamp01(cdTimer / cdTimerMax);

            t = Mathf.SmoothStep(0f, 1f, t);

            OnChangeProgress?.Invoke(this, new IHasClockTimer.OnChangeProgressEventArgs { progressNormalized = t });

            cdTimer -= Time.deltaTime;

            yield return null;

        }

        OnChangeProgress?.Invoke(this, new IHasClockTimer.OnChangeProgressEventArgs { progressNormalized = 0f });

        yield return null;

        ShowNoti();

        isCanUseAbility = true;

        yield return new WaitForSeconds(0.5f);

        HideNoti();

    }

    private void ShowNoti() {
        readyNotiText.gameObject.SetActive(true);
    }

    private void HideNoti() {
        readyNotiText.gameObject.SetActive(false);
    }

    public bool IsCanUseAbility() {
        return this.isCanUseAbility;
    }

    public void OnPointerEnter(PointerEventData eventData) {

        abilityIntroductionUI.gameObject.SetActive(true);

        Sequence introductionSequence = DOTween.Sequence();

        abilityIntroductionUI.DOKill();

        // 1. First Pos
        abilityIntroductionUI.localScale = Vector3.zero;
        abilityIntroductionUICanvas.alpha = 0f;

        // 2. DOTween Progress
        introductionSequence.Append(abilityIntroductionUI.DOScale(scaleValue, 0.2f).SetEase(Ease.OutBack));
        introductionSequence.Join(abilityIntroductionUICanvas.DOFade(1f, 0.2f));

    }

    public void OnPointerExit(PointerEventData eventData) {

        Sequence introductionSequence = DOTween.Sequence();

        abilityIntroductionUI.DOKill();
        abilityIntroductionUICanvas.DOKill();

        float currentScaleValue = abilityIntroductionUI.localScale.x;
        float timeLeft = 0.2f * Mathf.Clamp01(currentScaleValue / scaleValue);

        // 2. DOTween Progress
        introductionSequence.Append(abilityIntroductionUI.DOScale(0f, timeLeft).SetEase(Ease.InBack)).AppendCallback(() => {
            abilityIntroductionUI.gameObject.SetActive(false);
        });
        introductionSequence.Join(abilityIntroductionUICanvas.DOFade(0f, timeLeft));

    }
}
