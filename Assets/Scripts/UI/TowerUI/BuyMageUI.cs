using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuyMageUI : MonoBehaviour, IHasFunctionButton
{
    public event EventHandler OnFunctionButton;
    public event EventHandler<IHasFunctionButton.OffFunctionButtonEventArgs> OffFunctionButton;

    [Header("Arrow Button")]
    [SerializeField] private Button upArrowButton;
    [SerializeField] private Button rightArrowButton;
    [SerializeField] private Button downArrowButton;
    [SerializeField] private Button leftArrowButton;

    [Header("Text")]
    [SerializeField] private List<TextMeshProUGUI> priceTextList;

    private MageTower mageTower;
    private MageTowerUI mageTowerUI;

    private void Awake() {

        this.mageTower = GetComponentInParent<MageTower>();
        this.mageTowerUI = this.mageTower.GetComponentInChildren<MageTowerUI>();

        // Spawn Mage Button
        upArrowButton.onClick.AddListener(() => {
            BuyMage(SoldierSO.SoldierDirection.Up);
        });

        rightArrowButton.onClick.AddListener(() => {
            BuyMage(SoldierSO.SoldierDirection.Right);
        });

        downArrowButton.onClick.AddListener(() => {

            BuyMage(SoldierSO.SoldierDirection.Down);
        });

        leftArrowButton.onClick.AddListener(() => {

            BuyMage(SoldierSO.SoldierDirection.Left);

        });

    }

    private void Start() {

        mageTower.UnBuyMageUI += MageTower_UnBuyMageUI;
        mageTower.DeselectedAllUI += ArcherTower_DeselectedAllUI;

        // After spawn
        OnFunctionButton?.Invoke(this, EventArgs.Empty);

        UpdateVisual();
    }

    private void OnDestroy() {
        mageTower.UnBuyMageUI -= MageTower_UnBuyMageUI;
        mageTower.DeselectedAllUI -= ArcherTower_DeselectedAllUI;
    }

    private void ArcherTower_DeselectedAllUI(object sender, EventArgs e) {
        // When deselected all UI

        if (this.gameObject.activeSelf) {
            OffFunctionButton?.Invoke(this, new IHasFunctionButton.OffFunctionButtonEventArgs { callbackAction = OnDestroySelf });
        }

    }

    private void MageTower_UnBuyMageUI(object sender, EventArgs e) {
        OffFunctionButton?.Invoke(this, new IHasFunctionButton.OffFunctionButtonEventArgs { callbackAction = OnDestroySelf });
    }


    private void OnDestroySelf() {
        Destroy(this.gameObject);
    }

    private void BuyMage(SoldierSO.SoldierDirection mageDirection) {

        if (LevelManager.Instance.GetCurrentCoin() >= mageTower.GetMageTowerSO().priceMage) {

            Mage mage = Mage.SpawnMage(mageTower.GetMageTowerSO().soldierManagerSO, mageTower.GetSpawnPoint(mageDirection), mageDirection);

            mageTower.AddToMageList(mage);

            UpdateVisual();

            LevelManager.Instance.ChangedCoinTo(ILevelManager.CoinChangedState.Decrease, mageTower.GetMageTowerSO().priceMage);
        }

    }

    private void UpdateVisual() {

        List<Mage> mageList = mageTower.GetMageList();

        if (mageList.Count > 0 && mageList.Count < 4) {
            // If number of archer in list > 0 and < 4

            foreach (Mage mage in mageList) {

                if (mage.GetMageDirection() == SoldierSO.SoldierDirection.Up) {
                    // Nếu đã có archer hướng Up

                    upArrowButton.gameObject.SetActive(false);

                }

                if (mage.GetMageDirection() == SoldierSO.SoldierDirection.Right) {
                    // Nếu đã có archer hướng Right

                    rightArrowButton.gameObject.SetActive(false);

                }

                if (mage.GetMageDirection() == SoldierSO.SoldierDirection.Down) {
                    // Nếu đã có archer hướng Down

                    downArrowButton.gameObject.SetActive(false);

                }

                if (mage.GetMageDirection() == SoldierSO.SoldierDirection.Left) {
                    // Nếu đã có archer hướng Left

                    leftArrowButton.gameObject.SetActive(false);

                }

            }
        }
        else if (mageList.Count == 4) {
            // If number of archer in list = 4

            mageTowerUI.ShowUI();

            OffFunctionButton?.Invoke(this, new IHasFunctionButton.OffFunctionButtonEventArgs { callbackAction = OnDestroySelf });

        }
        else {
            // If number of archer in list = 0

            upArrowButton.gameObject.SetActive(true);
            rightArrowButton.gameObject.SetActive(true);
            downArrowButton.gameObject.SetActive(true);
            leftArrowButton.gameObject.SetActive(true);

        }

        foreach (TextMeshProUGUI priceText in priceTextList) {
            priceText.text = $"{this.mageTower.GetMageTowerSO().priceMage}$";
        }
    }

    public static BuyMageUI SpawnBuyMageUI(Transform buyMageUIPrefab, Transform parent) {

        Transform buyMageUITranform = Instantiate(buyMageUIPrefab, parent);
        buyMageUITranform.localPosition = Vector3.zero;

        BuyMageUI buyMageUI = buyMageUITranform.GetComponent<BuyMageUI>();

        return buyMageUI;
    }
}
