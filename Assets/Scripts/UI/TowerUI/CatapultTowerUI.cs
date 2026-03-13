using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CatapultTowerUI : MonoBehaviour, IHasFunctionButton {

    public event EventHandler OnFunctionButton;
    public event EventHandler<IHasFunctionButton.OffFunctionButtonEventArgs> OffFunctionButton;

    public event EventHandler OnFixingTower;

    [Header("Parent object")]
    [SerializeField] private CatapultTower catapultTower;

    [Header("Button")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button sellButton;
    [SerializeField] private Button fixButton;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI upgradePriceText;
    [SerializeField] private TextMeshProUGUI fixedPriceText;    

    private void Awake() {

        upgradeButton.onClick.AddListener(() => {

            if (catapultTower.IsUpgrading()) {
                // If is in upgrade progress
                return;
            }

            ITowerObject.LevelTower nextLevel = (ITowerObject.LevelTower)((int)catapultTower.GetCurrentTowerStatus().levelTower + 1);
            CatapultTowerLevelData nextLevelData = catapultTower.GetCatapulTowerDataDict()[nextLevel];

            if (LevelManager.Instance.GetCurrentCoin() < nextLevelData.upgradeCost) {
                // If current Coin in game isn't enough for upgrade tower

                return;
            }
            else {
                // If enough Coin

                LevelManager.Instance.ChangedCoinTo(ILevelManager.CoinChangedState.Decrease, nextLevelData.upgradeCost);
            }

            int currentLevelIndex = (int)catapultTower.GetCurrentTowerStatus().levelTower;
            int nextLevelIndex = currentLevelIndex + 1;

            catapultTower.ChangeStateTo((ITowerObject.LevelTower)nextLevelIndex);

        });


        sellButton.onClick.AddListener(() => {


            LevelManager.Instance.ChangedCoinTo(ILevelManager.CoinChangedState.Increase, catapultTower.GetCurrentPrice());

            GridManager.Instance.SetHasItemArea2x2(this.transform.position, isSetHasItemON: false);

            Destroy(catapultTower.transform.gameObject);

        });

        fixButton.onClick.AddListener(() => {

            if (LevelManager.Instance.GetCurrentCoin() < catapultTower.GetCatapultTowerSO().fixedCost) {
                // If not enough coin to fixed

                return;
            }

            // Start fixing
            OnFixingTower?.Invoke(this, EventArgs.Empty);

        });
    }


    private void Start() {

        catapultTower.OnCatapultTowerUI += CatapultTower_OnCatapultTowerUI;
        catapultTower.UnCatapultTowerUI += CatapultTower_UnCatapultTowerUI;
        catapultTower.DeselectedAllUI += CatapultTower_DeselectedAllUI;

        HideUI();
    }


    private void OnDestroy() {
        catapultTower.OnCatapultTowerUI -= CatapultTower_OnCatapultTowerUI;
        catapultTower.UnCatapultTowerUI -= CatapultTower_UnCatapultTowerUI;
        catapultTower.DeselectedAllUI -= CatapultTower_DeselectedAllUI;
    }


    private void CatapultTower_DeselectedAllUI(object sender, EventArgs e) {
        // When deselected all UI

        if (this.gameObject.activeSelf) {
            OffFunctionButton?.Invoke(this, new IHasFunctionButton.OffFunctionButtonEventArgs { callbackAction = HideUI });

        }


    }

    private void CatapultTower_UnCatapultTowerUI(object sender, System.EventArgs e) {

        OffFunctionButton?.Invoke(this, new IHasFunctionButton.OffFunctionButtonEventArgs { callbackAction = HideUI });

    }

    private void CatapultTower_OnCatapultTowerUI(object sender, System.EventArgs e) {

        ShowUI();
        UpdateVisual();

    }

    private void UpdateVisual() {

        // 1. Upgrade Button 
        if (catapultTower.GetCurrentTowerStatus().levelTower == ITowerObject.LevelTower.Level4) {
            // If tower reached last level

            upgradeButton.gameObject.SetActive(false);

        }
        else {
            // Is tower not reached last level

            upgradeButton.gameObject.SetActive(true);

            ITowerObject.LevelTower nextLevel = (ITowerObject.LevelTower)((int)catapultTower.GetCurrentTowerStatus().levelTower + 1);
            CatapultTowerLevelData nextLevelData = catapultTower.GetCatapulTowerDataDict()[nextLevel];

            upgradePriceText.text = $"{nextLevelData.upgradeCost}$";
        }

        // 2. Fixed Button
        fixedPriceText.text = $"{catapultTower.GetCatapultTowerSO().fixedCost}$";

    }

    private void HideUI() {
        this.gameObject.SetActive(false);
    }

    public void ShowUI() {

        this.gameObject.SetActive(true);

        OnFunctionButton?.Invoke(this, EventArgs.Empty);
    }
}
