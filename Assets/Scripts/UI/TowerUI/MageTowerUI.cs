using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MageTowerUI : MonoBehaviour, IHasFunctionButton
{
    public event EventHandler OnFunctionButton;
    public event EventHandler<IHasFunctionButton.OffFunctionButtonEventArgs> OffFunctionButton;

    public event EventHandler OnFixingTower;

    [Header("Parent object")]
    [SerializeField] private MageTower mageTower;

    [Header("Button")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button buyMageButton;
    [SerializeField] private Button sellButton;
    [SerializeField] private Button fixButton;
    [SerializeField] private Button changeTypeButton;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI typeNameText;
    [SerializeField] private TextMeshProUGUI upgradeCostText;
    [SerializeField] private TextMeshProUGUI fixedCostText;

    [Header("UI")]
    [SerializeField] private Transform buyMageUIPrefab;

    [Header("Mage Tower Type Swipe")]
    [SerializeField] private Image[] towerTypeImageArray;
    [SerializeField] RectTransform levelPagesRect;
    [SerializeField] float tweenTime;
    [SerializeField] LeanTweenType tweenType;

    private const string GROUND_TYPE_NAME = "GROUND";
    private const string SKY_TYPE_NAME = "SKY";
    private const string RIVER_TYPE_NAME = "RIVER";

    private BuyMageUI buyMageUI;

    private int currentPage = 0;
    private int totalPage;
    private Vector3 nextStep;
    private Vector3 targetPos;

    private void Awake() {

        totalPage = towerTypeImageArray.Length;
        targetPos = levelPagesRect.localPosition;

        float posX = -(towerTypeImageArray[0].GetComponent<RectTransform>().sizeDelta.x + levelPagesRect.GetComponent<HorizontalLayoutGroup>().spacing);
        nextStep = new Vector3(posX, 0f, 0f);


        upgradeButton.onClick.AddListener(() => {

            if (mageTower.IsUpgrading()) {
                // If is in upgrade progress
                return;
            }

            List<MageTowerLevelData> mageTowerLevelDataList = mageTower.GetMageTowerSO().towerLevelDataList;
            float upgradeNextLevelCost = mageTowerLevelDataList[mageTowerLevelDataList.IndexOf(mageTower.GetCurrentTowerStatus()) + 1].upgradeCost;

            if (LevelManager.Instance.GetCurrentCoin() < upgradeNextLevelCost) {
                return;
            }

            LevelManager.Instance.ChangedCoinTo(ILevelManager.CoinChangedState.Decrease, upgradeNextLevelCost);

            int currentLevelIndex = (int)mageTower.GetCurrentTowerStatus().levelTower;
            int nextLevelIndex = currentLevelIndex + 1;

            mageTower.ChangeStateTo((ITowerObject.LevelTower)nextLevelIndex);

        });

        buyMageButton.onClick.AddListener(() => {

            buyMageUI = BuyMageUI.SpawnBuyMageUI(buyMageUIPrefab, this.transform.parent);

            OffFunctionButton?.Invoke(this, new IHasFunctionButton.OffFunctionButtonEventArgs { callbackAction = HideUI });

        });

        sellButton.onClick.AddListener(() => {


            LevelManager.Instance.ChangedCoinTo(ILevelManager.CoinChangedState.Increase, mageTower.GetCurrentPrice());

            GridManager.Instance.SetHasItemArea2x2(this.transform.position, isSetHasItemON: false);

            Destroy(mageTower.transform.gameObject);

        });

        fixButton.onClick.AddListener(() => {

            // Start fixing
            OnFixingTower?.Invoke(this, EventArgs.Empty);

        });

        changeTypeButton.onClick.AddListener(() => {

            if (currentPage < totalPage - 1) {
                currentPage += 1;
                targetPos += nextStep;

            }
            else if (currentPage == totalPage - 1) {

                currentPage = 0;
                targetPos -= nextStep * (totalPage - 1);

            }

            mageTower.ChangeMageTowerTypeTo((MageTowerSO.MageType)currentPage);

            levelPagesRect.LeanMoveLocal(targetPos, tweenTime).setEase(tweenType);

            UpdateVisual();

        });
    }


    private void Start() {

        mageTower.OnMageTowerUI += MageTower_OnMageTowerUI;
        mageTower.UnMageTowerUI += MageTower_UnMageTowerUI;
        mageTower.DeselectedAllUI += MageTower_DeselectedAllUI;

        UpdateVisual();

        HideUI();
    }


    private void OnDestroy() {
        mageTower.OnMageTowerUI -= MageTower_OnMageTowerUI;
        mageTower.UnMageTowerUI -= MageTower_UnMageTowerUI;
        mageTower.DeselectedAllUI -= MageTower_DeselectedAllUI;

    }


    private void MageTower_DeselectedAllUI(object sender, EventArgs e) {
        // When deselected all UI

        if (this.gameObject.activeSelf) {
            OffFunctionButton?.Invoke(this, new IHasFunctionButton.OffFunctionButtonEventArgs { callbackAction = HideUI });

            buyMageUI = null;
        }


    }

    private void MageTower_UnMageTowerUI(object sender, System.EventArgs e) {

        OffFunctionButton?.Invoke(this, new IHasFunctionButton.OffFunctionButtonEventArgs { callbackAction = HideUI });

    }

    private void MageTower_OnMageTowerUI(object sender, System.EventArgs e) {

        ShowUI();

        buyMageUI = null;

    }

    private void UpdateVisual() {

        if (mageTower.GetCurrentMageType() == MageTowerSO.MageType.Ground) {
            typeNameText.text = $"{GROUND_TYPE_NAME}";
        }
        else if (mageTower.GetCurrentMageType() == MageTowerSO.MageType.Sky) {
            typeNameText.text = $"{SKY_TYPE_NAME}";
        }
        else if (mageTower.GetCurrentMageType() == MageTowerSO.MageType.River){
            typeNameText.text = $"{RIVER_TYPE_NAME}";
        }

        upgradeCostText.text = $"{mageTower.GetCurrentTowerStatus().upgradeCost}$";
        fixedCostText.text = $"{mageTower.GetMageTowerSO().fixedCost}$";
    }

    private void HideUI() {
        this.gameObject.SetActive(false);
    }

    public void ShowUI() {

        this.gameObject.SetActive(true);


        if (mageTower.GetMageList().Count == 4) {
            buyMageButton.gameObject.SetActive(false);
        }

        // Update upgradeLevelButton visual and text
        if (mageTower.GetCurrentTowerStatus().levelTower == ITowerObject.LevelTower.Level4) {
            // Reached last level

            upgradeButton.gameObject.SetActive(false);
        }
        else {

            List<MageTowerLevelData> mageTowerLevelDataList = mageTower.GetMageTowerSO().towerLevelDataList;
            float upgradeNextLevelCost = mageTowerLevelDataList[mageTowerLevelDataList.IndexOf(mageTower.GetCurrentTowerStatus()) + 1].upgradeCost;

            upgradeCostText.text = $"{upgradeNextLevelCost}$";
        }

        fixedCostText.text = $"{mageTower.GetMageTowerSO().fixedCost}$";

        OnFunctionButton?.Invoke(this, EventArgs.Empty);
    }

    public bool HasBuyMageUI() {
        return this.buyMageUI != null;
    }
}
