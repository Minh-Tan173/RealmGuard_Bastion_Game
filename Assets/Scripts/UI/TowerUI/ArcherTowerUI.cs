using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class ArcherTowerUI : MonoBehaviour, IHasFunctionButton {

    public event EventHandler OnFunctionButton;
    public event EventHandler<IHasFunctionButton.OffFunctionButtonEventArgs> OffFunctionButton;

    public event EventHandler OnFixingTower;

    [Header("Parent object")]
    [SerializeField] private ArcherTower archerTower;

    [Header("Button")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button buyArcherButton;
    [SerializeField] private Button sellButton;
    [SerializeField] private Button fixButton;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI upgradeCostText;
    [SerializeField] private TextMeshProUGUI fixedCostText;

    [Header("UI")]
    [SerializeField] private Transform buyArcherUIPrefab;

    private BuyArcherUI buyArcherUI;

    private void Awake() {

        upgradeButton.onClick.AddListener(() => {

            if (archerTower.IsUpgrading()) {
                // If is in upgrade progress
                return;
            }

            int currentLevelIndex = (int)archerTower.GetCurrentTowerStatus().levelTower;
            int nextLevelIndex = currentLevelIndex + 1;

            archerTower.ChangeLevelTo((ITowerObject.LevelTower)nextLevelIndex);

        });

        buyArcherButton.onClick.AddListener(() => {

            buyArcherUI = BuyArcherUI.SpawnBuyArcherUI(buyArcherUIPrefab, this.transform.parent);

            OffFunctionButton?.Invoke(this, new IHasFunctionButton.OffFunctionButtonEventArgs { callbackAction = HideUI });

        });

        sellButton.onClick.AddListener(() => {


            LevelManager.Instance.ChangedCoinTo(ILevelManager.CoinChangedState.Increase, archerTower.GetArcherTowerSO().price);

            GridManager.Instance.SetHasItemArea2x2(this.transform.position, isSetHasItemON: false);

            Destroy(archerTower.transform.gameObject);

        });

        fixButton.onClick.AddListener(() => {

            // Start fixing
            OnFixingTower?.Invoke(this, EventArgs.Empty);

        });
    }


    private void Start() {

        archerTower.OnArcherTowerUI += ArcherTower_OnArcherTowerUI;
        archerTower.UnArcherTowerUI += ArcherTower_UnArcherTowerUI;
        archerTower.DeselectedAllUI += ArcherTower_DeselectedAllUI;
 
        HideUI();
    }


    private void OnDestroy() {
        archerTower.OnArcherTowerUI -= ArcherTower_OnArcherTowerUI;
        archerTower.UnArcherTowerUI -= ArcherTower_UnArcherTowerUI;
        archerTower.DeselectedAllUI -= ArcherTower_DeselectedAllUI;
    }


    private void ArcherTower_DeselectedAllUI(object sender, EventArgs e) {
        // When deselected all UI

        if (this.gameObject.activeSelf) {
            OffFunctionButton?.Invoke(this, new IHasFunctionButton.OffFunctionButtonEventArgs { callbackAction = HideUI });

            buyArcherUI = null; 
        }
        

    }

    private void ArcherTower_UnArcherTowerUI(object sender, System.EventArgs e) {

        OffFunctionButton?.Invoke(this, new IHasFunctionButton.OffFunctionButtonEventArgs { callbackAction = HideUI });

    }

    private void ArcherTower_OnArcherTowerUI(object sender, System.EventArgs e) {

        ShowUI();

        buyArcherUI = null;

    }

    private void UpdateVisual() {

        upgradeCostText.text = $"{archerTower.GetCurrentTowerStatus().upgradeCost}$";
        fixedCostText.text = $"{archerTower.GetArcherTowerSO().fixedCost}$";
    }

    private void HideUI() {
        this.gameObject.SetActive(false);
    }

    public void ShowUI() {

        UpdateVisual();

        this.gameObject.SetActive(true);

        if (archerTower.GetArcherList().Count == 4) {
            buyArcherButton.gameObject.SetActive(false);
        }

        OnFunctionButton?.Invoke(this, EventArgs.Empty);
    }

    public bool HasBuyArcherUI() {
        return this.buyArcherUI != null;
    }

}
