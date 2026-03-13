using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TowerButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IHasPadLock
{

    public event EventHandler<IHasPadLock.UnlockPadLockEventArgs> UnlockPadLock;

    [Header("Tower Data")]
    [SerializeField] private ITowerObject.TowerType towerType;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI towerPriceText;

    [Header("Introduction UI")]
    [SerializeField] private RectTransform towerIntroductionUI;
    [SerializeField] private CanvasGroup towerIntroductionUICanvas;
    [SerializeField] private float scaleValue;

    [Header("PadLock")]
    [SerializeField] private PadLock padLock;

    private Button button;
    private bool isUnlocked = false;

    private void Awake() {

        button = GetComponent<Button>();

        button.onClick.AddListener(() => {
            BuyTower();
        });

    }

    private void Start() {

        towerIntroductionUI.transform.gameObject.SetActive(false);

        LevelManager.Instance.UnlockTowerButton += LevelManager_UnlockTowerButton;

        // Update Tower Price Text
        towerPriceText.text = $"{TowerManager.Instance.GetTowerSOByType(towerType).price}$";

        if (SaveData.IsTowerUnlocked(towerType)) {
            // If this tower has unlocked

            Unlock();

        }
        else {
            Lock();
        }
    }

    private void OnDestroy() {

        LevelManager.Instance.UnlockTowerButton -= LevelManager_UnlockTowerButton;
    }

    private void LevelManager_UnlockTowerButton(object sender, LevelManager.UnLockTowerButtonEventArgs e) {

        if (this.towerType == e.towerType) {
            // Unlock this button

            UnlockPadLock?.Invoke(this, new IHasPadLock.UnlockPadLockEventArgs {
                callbackAction = () => {
                    
                    Unlock();

                }
            });
        }
    }

    private void BuyTower() {

        if (BuildingManager.Instance.IsPlacingTower() || BuildingManager.Instance.IsPlacingAbility()) {
            // If has towerIcon on mouse
            BuildingManager.Instance.ForceReset();

            return;
        }

        TowerSO towerSO = TowerManager.Instance.GetTowerSOByType(towerType);

        if (LevelManager.Instance.GetCurrentCoin() >= towerSO.price) {

            BuildingManager.Instance.SpawnTowerIcon(towerSO);

            LevelManager.Instance.ChangedCoinTo(ILevelManager.CoinChangedState.Decrease, towerSO.price);

        }

    }

    private void Unlock() {
        button.interactable = true;
        isUnlocked = true;
        padLock.gameObject.SetActive(false);
    }

    private void Lock() {
        button.interactable = false;
        padLock.gameObject.SetActive(true);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        // When mouse move on this button

        if (!isUnlocked) { return; }

        towerIntroductionUI.gameObject.SetActive(true);

        Sequence introductionSequence = DOTween.Sequence();

        towerIntroductionUI.DOKill();

        // 1. First Pos
        towerIntroductionUI.localScale = Vector3.zero;
        towerIntroductionUICanvas.alpha = 0f;

        // 2. DOTween Progress
        introductionSequence.Append(towerIntroductionUI.DOScale(scaleValue, 0.2f).SetEase(Ease.OutBack));
        introductionSequence.Join(towerIntroductionUICanvas.DOFade(1f, 0.2f));
    }

    public void OnPointerExit(PointerEventData eventData) {
        // When mouse move left this button

        if (!isUnlocked) { return; }

        Sequence introductionSequence = DOTween.Sequence();

        towerIntroductionUI.DOKill();
        towerIntroductionUICanvas.DOKill();

        float currentScaleValue = towerIntroductionUI.localScale.x;
        float timeLeft = 0.2f * Mathf.Clamp01(currentScaleValue / scaleValue);

        // 2. DOTween Progress
        introductionSequence.Append(towerIntroductionUI.DOScale(0f, timeLeft).SetEase(Ease.InBack)).AppendCallback(() => {
            towerIntroductionUI.gameObject.SetActive(false);    
        });
        introductionSequence.Join(towerIntroductionUICanvas.DOFade(0f, timeLeft));
    }
}
